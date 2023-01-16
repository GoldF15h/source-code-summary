# เว็บอ้างอิงสำหรับค่า lat long
# https://en.wikipedia.org/wiki/List_of_districts_of_Thailand

import requests
import pandas as pd
import os

def get_between(_string,_head,_tail):
    head = _string.find(_head)
    tail = head + _string[head::].find(_tail)
    return _string[head:tail:]

def extract_link_and_name (input_string):
    input_list = input_string.split('"')
    # print(input_list)
    link_string = input_list[1]
    name = input_list[5]
    return [link_string,name]

def get_geo(geo_link):
    page = requests.get('https://en.wikipedia.org'+geo_link).text
    open('tmp.html','w').write(page)
    latitude_tag = get_between(page,'<span class="latitude">','</span>')
    latitude = latitude_tag.split('>')[1]
    # print(latitude)
    longitude_tag = get_between(page,'<span class="longitude">','</span>')
    longtitude = longitude_tag.split('>')[1]
    return [latitude,longtitude]

if __name__ == "__main__" :
    main_page = requests.get('https://en.wikipedia.org/wiki/List_of_districts_of_Thailand').text
    
    # table_head = main_page.find('<table')
    # table_tail = main_page.find('</table>')
    # print(table_head,table_tail)
    # main_table = main_page[table_head:table_tail:]
    main_table = get_between(main_page,'<table','</table>')

    # print(main_table)
    main_table = main_table.split('<td>')[1::]
    # print(len(main_table)/5)
    # print( '\n*****'.join(main_table[:5:]) )

    data = {
            'district' : [],
            'district_lat' : [],
            'district_long' : [],
            'province' : [],
            'province_lat' : [],
            'province_long' : [],
        }

    _len = len(main_table)
    total_skip = 0
    # err index 260
    for i in range(0,_len,5) :
        os.system('clear')
        print(f'process : {i/_len:.4f}/100 , index : {i}/{_len} , total skip : {total_skip}')
        district_string = main_table[i]
        province_string = main_table[i+2]

        district_geo_link,district_name = extract_link_and_name(district_string)
        province_geo_link,province_name = extract_link_and_name(province_string)
        # print(district_geo_link,district_name,province_geo_link,province_name)

        try :
            district_latitude,district_longitude = get_geo(district_geo_link)
            province_latitude,province_longitude = get_geo(province_geo_link)
        except :
            total_skip += 1
            continue

        # print(district_latitude,district_longitude,province_latitude,province_longitude)

        data['district'].append(district_name)
        data['district_lat'].append(district_latitude)
        data['district_long'].append(district_longitude)
        data['province'].append(province_name)
        data['province_lat'].append(province_latitude)
        data['province_long'].append(province_longitude)

    df = pd.DataFrame(data)
    df.to_csv('lat_long_data.csv')

