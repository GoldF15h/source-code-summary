import pandas as pd
import datetime as dt
import os
import matplotlib.pyplot as plt
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LinearRegression
from sklearn import metrics, datasets
import seaborn as sns

# อ่านไฟล์
# df = pd.read_csv('../raw-data/event2019.csv')
df = pd.read_csv("../raw-data/2018_2021_District_Province/2018District.csv")

# เลือกข้อมูลที่ type == 3
df = df[df["type"] == 3]
# print(df["type"])

# drop คอลั่มที่ไม่ใช่
df = df.drop(["eid", "title", "title_en", "description",
             "description_en", "stop", "contributor"], axis=1)
# print(df)

# ดึงวันที่ และ ชั่วโมงออกมาจากคอลั่ม start

# 2019-12-31 21:54:15


def extract_date(x):
    y, m, d = x.split(' ')[0].split('-')
    return '/'.join([d, m, y])


def extract_hour(x):
    return x.split(' ')[1].split(':')[0]


def extract_day(x):
    weekday = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
    try:
        y, m, d = list(map(int, x.split(' ')[0].split('-')))
    except:
        d, m, y = list(map(int, x.split('/')))
    day_index = dt.datetime(y, m, d).weekday()
    return weekday[day_index]


df["date"] = df["start"].apply(extract_date)
df["hour"] = df["start"].apply(extract_hour)
df["day"] = df["start"].apply(extract_day)
# print(df[["date","hour","start"]])

# drop คอลั่มที่ไม่จำเป็นและทำการสร้างคอลั่มใหม่
df = df.drop(["start", 'type'], axis=1)
df['weather'] = 'sunny'
# df = df[['date',	'day',	'province',	'district',
#         'hour',	'weather',	'latitude',	'longitude']].reset_index()
# df = df.drop(['index'], axis=1)
# df.to_csv("../raw-data/data2018.csv")

df['province'] = None
df['district'] = None
df['frequency'] = 1

# print(df.columns)

# จัด คอลั่มใหม่
df = df[["date", 'day', "province", "district", "hour",
         "frequency", "weather", "latitude", "longitude"]]

# print(df.info())cd
# print(df)
# df.to_csv('tmp.csv', index=False)
# print(datetime.datetime.today().weekday())

# freq per hour
freq_per_hour = df.groupby('hour').count()['frequency']
# print(freq_per_hour)

# freq per day
freq_per_day = df.groupby('day').count()['frequency']
# print(freq_per_day)

# freq per date and hour
freq_per_date_and_hour = pd.DataFrame()
freq_per_date_and_hour['date'] = pd.date_range(
    '2019-01-01 00:00:00', '2019-12-31 23:59:59', freq='H').strftime("%d/%m/%Y:%H").tolist()

tmp_df = pd.DataFrame()
tmp_df['date'] = df["date"] + ':' + df['hour']
tmp_df['freq'] = 0
# print(tmp_df)

freq_per_date_and_hour['freq'] = 0

freq_per_date_and_hour = pd.concat(
    [freq_per_date_and_hour, tmp_df], ignore_index=True)
freq_per_date_and_hour = freq_per_date_and_hour.groupby('date').count() - 1
freq_per_date_and_hour = freq_per_date_and_hour.reset_index()
freq_per_date_and_hour['hour'] = freq_per_date_and_hour['date'].apply(
    lambda x: int(x.split(':')[1]))
freq_per_date_and_hour['date'] = freq_per_date_and_hour['date'].apply(
    lambda x: x.split(':')[0])
freq_per_date_and_hour = freq_per_date_and_hour[['date', 'hour', 'freq']]
freq_per_date_and_hour["day"] = freq_per_date_and_hour["date"].apply(
    extract_day)
# print(freq_per_date_and_hour)


freq_df = freq_per_date_and_hour[['day', 'date', 'hour',
                                  'freq']]
freq_df['ความถี่ของอุบัติเหตุ (เปอร์เซ็นท์)'] = freq_df['freq'] / \
    freq_df['freq'].abs().max()

day_grouped = freq_df.sort_values(
    by=['date', 'hour']).groupby(df['day'])


df_mon = day_grouped.get_group("Mon")
df_tue = day_grouped.get_group("Tue")
df_wed = day_grouped.get_group("Wed")
df_thu = day_grouped.get_group("Thu")
df_fri = day_grouped.get_group("Fri")
df_sat = day_grouped.get_group("Sat")
df_sun = day_grouped.get_group("Sun")
# print(df_fri.to_string())

model_df = df_fri[['hour', 'ความถี่ของอุบัติเหตุ (เปอร์เซ็นท์)']]


# Plot
# df_mon.plot(x="วันที่", y='ความถี่ของอุบัติเหตุ (เปอร์เซ็นท์)', style='o')
# plt.title('แนวโน้มการเกิดอุบัติเหตุในวันจันทร์')
# plt.xlabel('วันที่')
# plt.ylabel('ความถี่ของอุบัติเหตุ (เปอร์เซ็นท์)')
# plt.show()


X = model_df.iloc[:, :-1].values
y = model_df['ความถี่ของอุบัติเหตุ (เปอร์เซ็นท์)'].values

X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=0)

regressor = LinearRegression()
regressor.fit(X_train, y_train)

y_pred = regressor.predict(X_test)

reg_df = pd.DataFrame({'Actual': y_test, 'Predicted': y_pred})

print('Mean Absolute Error:', metrics.mean_absolute_error(y_test, y_pred))
print('Mean Squared Error:', metrics.mean_squared_error(y_test, y_pred))
print('Root Mean Squared Error:', np.sqrt(
    metrics.mean_squared_error(y_test, y_pred)))

plt.scatter(X_test, y_test, color="black")
plt.plot(X_test, y_pred, color="blue", linewidth=3)

# sns.scatterplot(data=reg_df,x=X_test,y=y_test);
# sns.regplot(x=X_test, y=y_pred, data=reg_df);

plt.title('Linear Regression')
plt.xlabel('Date (from Jan to Dec)')
plt.ylabel('Frequency (Percentage)')
plt.xticks(())
plt.yticks(())

plt.show()
# df.to_csv('./out/data.csv',index=False)
# freq_df.to_csv('./out/freq.csv',index=False)
