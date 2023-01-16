from distutils.log import error
import re
import pandas as pd
import sys

COIN_DIV_CLASS_NAME = ''
COIN_DATA_EXTRACT = ''
# <div.*?>(.*?)<\/div> # เอาข้อมูล child ออกมา
# >(.*?)< # อันนี้สั้นกว่าข้อมูลครบเหมือนกันได้ช่วงที่ต้องการมาเลย

# ฟังก์ชั่นนี้รับข้อมูลเป็น str ส่วนของ table และทำการใช้ regex นำข้อมูลออกมา และส่งกลับคืนเป็น python dict
# โดยมี key ดังนี้
# "short_name" คือ ชื่อย่อของเหรียญ
# "full_name" คือ ชื่อเต็มของเหรียญ
# "price" คือ ราคาปัจจุบัน
# "change" คือ การเปลี่ยนแปลงของราคาในช่วง 24 ชม
# "volume" คือ volume ในช่วง 24 ชม
# "market_cap" คือ market_cap

# และในการเข้าถึงข้อมูลสามารถทำได้โดยใช้ค่า index เนื่องจากตั้งให้ key ตรงกับเลข index เพื่อความง่ายในการใช้งาน

# ในการใช้งานไฟล์สามารถดูได้จากไฟล์ custom_regex_example.py ได้


def extract_coin_data(data, option):

    try:
        if option == 'bn':
            # print("\n\nBINANCE")
            coin_data = extract_binance_coin_data(data)
        elif option == 'bm':
            # print("\n\nBITMART")
            coin_data = extract_bitmart_coin_data(data)
        elif option == 'g':
            print("\n\nGATE")
            coin_data = extract_gate_coin_data(data)
        elif option == 'kc':
            # print("\n\nKUCOIN")
            coin_data = extract_kucoin_coin_data(data)

    except Exception as e:
        print('CATCH! -> ', str(e))
        coin_data = pd.DataFrame()

    return coin_data


def extract_to_list(regex_str, data, level=3):
    l = re.findall(regex_str, data)
    for i in range(len(l)):
        l[i] = l[i][level]
    return l


def extract_binance_coin_data(coins_data):
    coin_dataFrame = pd.DataFrame()
    coin_dataFrame["name"] = extract_to_list(
        '(<div direction=\"ltr\"(.*?(>([A-Z ]+)<).*?)Trade)', coins_data)
    coin_dataFrame["price"] = extract_to_list(
        '(<div direction=\"ltr\"(.*?(>(\d.+?\d)<).*?)Trade)', coins_data)
    coin_dataFrame["change"] = extract_to_list(
        '(<div direction=\"ltr\"(.*?(>([+|-].*?)%<).*?)Trade)', coins_data)
    coin_dataFrame["high_low"] = extract_to_list(
        '(<div direction=\"ltr\"(.*?(>([+|-].*?)%<).*?))(>.*?)((.*?(>(.*?) / .*?<).*?)Trade)', coins_data, 8)
    coin_dataFrame["high_low"] += extract_to_list(
        '(<div direction=\"ltr\"(.*?(>.*? (/ .*?)<).*?)Trade)', coins_data)
    coin_dataFrame["volume"] = extract_to_list(
        '(<div direction=\"ltr\"(.*?(>.*? (/ .*?)<).*?))(title=\"(.*?[M|]\"))', coins_data, 5)

    return coin_dataFrame


def extract_gate_coin_data(coins_data):
    max_range = 500
    coin_dataFrame = pd.DataFrame()
    coin_dataFrame["name"] = extract_to_list(
        '(<tr .*?(.*?(>([A-Z ]+)<).*?)<\/tr>)', coins_data)[:max_range:]
    coin_dataFrame["price"] = extract_to_list(
        '<tr .*?(.*?(<span>(.*?)<).*?)<\/tr>', coins_data, 2)[:max_range:]
    coin_dataFrame["change"] = extract_to_list(
        '(<tr .*?(>([\+|\-].*?) %<).*?)<\/tr>', coins_data, 2)[:max_range:]

    l = extract_to_list(
        '<tr .*?>(.*?)></td><td>(.+?</td><td>\$.*?<.*?)<a.*?<\/tr>', coins_data, 1)[:max_range:]

    high_low_list = []
    volume_list = []

    for i in range(max_range):
        l[i] = re.findall('>(.*?)<', l[i])
        tmp = []
        for j in range(len(l[i])):
            if l[i][j] != '':
                tmp.append(l[i][j])
        l[i] = tmp[::]
        # print(i, l[i])

        volume_list.append(l[i][-1])
        high_low_list.append(l[i][-4] + '/' + l[i][-3])

    coin_dataFrame["high_low"] = high_low_list
    coin_dataFrame["volume"] = volume_list

    return coin_dataFrame


def extract_bitmart_coin_data(coins_data):
    coin_dataFrame = pd.DataFrame()
    max_range = 500

    l = re.findall(
        '<tr.*?\s{15}(.*?)\s{13}.*?.*?\s{14}(.*?)\s{12}.*?.*?\s{14}(.*?)\s{12}.*?.*?\s{14}(.*?)\s{1}.*?<\/tr>', coins_data)

    coin_dataFrame["name"] = re.findall(
        '<tr .*?>(\w.*?)/USD.*?<\/tr>', coins_data)[:max_range:]
    coin_dataFrame["price"] = re.findall(
        '<tr .*?>/ (.*?) USD.*?<\/tr>', coins_data)[:max_range:]

    # print(l[0])

    coin_dataFrame["change"] = [x[0] for x in l][:max_range:]
    coin_dataFrame["high_low"] = [x[1] + ' / ' + x[2] for x in l][:max_range:]
    coin_dataFrame["volume"] = [x[3] for x in l][:max_range:]
    return coin_dataFrame


def extract_kucoin_coin_data(coins_data):
    COIN_DIV_CLASS_NAME = '(<tr .*?<\/tr>)'
    coins_data = re.findall(COIN_DIV_CLASS_NAME, coins_data)
    COIN_DATA_EXTRACT = '>(.*?)<'
    coin_dataFrame = pd.DataFrame()

    name_list = []
    price_list = []
    change_list = []
    high_low_list = []
    volume_list = []
    marketcap_list = []

    for i in range(len(coins_data)):
        cur_str = str(coins_data[i])
        # print(cur_str)
        regex_resault = re.findall(COIN_DATA_EXTRACT, cur_str)

        l = []

        tmp_list = []
        for cur in regex_resault:
            if cur != '':
                tmp_list.append(cur)
        regex_resault = tmp_list[::]
        # print(regex_resault)
        # sys.exit()

        for i in range(len(regex_resault)):
            if regex_resault[i] != '' and regex_resault[i] != ' ' and regex_resault[i] != '0':
                l.append(str(regex_resault[i]).strip())
        # print("KU LEN :", len(l), l)
        if len(l) == 12:
            name, _, _, price1, price2, _, change, market_cap, volume, _, _, _ = l[
                ::]
        else:
            name, _, _, price1, price2, _, change, _, market_cap, volume, _, _, _ = l[
                ::]
        high_low = None

        name_list.append(name)
        price_list.append(price1)
        change_list.append(change)
        high_low_list.append(high_low)
        volume_list.append(volume)
        marketcap_list.append(market_cap)

    coin_dataFrame["name"] = name_list
    coin_dataFrame["price"] = price_list
    coin_dataFrame["change"] = change_list
    coin_dataFrame["high_low"] = high_low_list
    coin_dataFrame["volume"] = volume_list
    coin_dataFrame["marketcap"] = marketcap_list
    return coin_dataFrame


if __name__ == "__main__":
    # op = extract_coin_data(
    #     open('../results/binance1.txt', 'r').read(), option='bn')
    # op = extract_coin_data(
    #     open('../results/bitmart1.txt', 'r').read(), option='bm')
    # op = extract_coin_data(
    #     open('../results/gate1.txt', 'r').read(), option='g')
    op = extract_coin_data(
        open('../results/kucoin1.txt', 'r').read(), option='kc')
    print(op)

    # s = ''.join(open('../results/bitmart1.txt', 'r').read().split('\t'))
    # open('../results/bitmart1.txt', 'w').write(s)
