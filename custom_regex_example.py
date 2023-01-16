import custom_regex

if __name__ == "__main__":
    dummy_data = open("../results/binance1.txt", 'r').read()
    resault = custom_regex.extract_coin_data(dummy_data)
    for i in range(50):
        print(i, resault[i])
