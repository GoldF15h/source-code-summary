from pyautogui import click
from pyautogui import screenshot as sc
from numpy import array
from cv2 import cvtColor, COLOR_RGB2GRAY, destroyAllWindows
from keyboard import is_pressed
from os import system
import cv2
import time
import pyautogui

# game use in this porject
# https://poki.com/th/g/piano-tiles-2
# https://www.softgames.com/free-online-games/games/tap-the-black-tile
# Top Left      609   ,  120
# Bottom Right  1021  ,  883
# SIZE ->       775   X  413
# old setting (473,0,-50)

# piano tile setting
top_left = [170, 50]
bottom_right = [780, 780]
sampling_rate = 50

# tap the black tile
# top_left = [230,480]
# bottom_right = [730,780]
# sampling_rate = 100

top_x, top_y = top_left
width, height = bottom_right[0] - top_left[0], bottom_right[1] - top_left[1]

# defind what x coordinate value we need to check
sampling_space = width // 4
start_sampling_x_value = sampling_space//2
y_cord = [
    start_sampling_x_value,
    start_sampling_x_value+(1*sampling_space),
    start_sampling_x_value+(2*sampling_space),
    start_sampling_x_value+(3*sampling_space)
]
print(y_cord)
# y_cord = [50,150,250,350]


def black_scan_click(screenshot, y_cord, time):
    # scan for black color
    for x in range(height-1, 0, -(sampling_rate)):
        for y in y_cord:
            # if color is black
            if screenshot[x, y] < 10:
                click(top_x+y, top_y+x)
                return
            # screenshot[x,y] = 0
            # screenshot[x-1,y] = 255


if __name__ == "__main__":

    print("BOT IS RUNNING")

    # count how many time we click
    time = 0

    # start
    while True:
        # take Screenshot
        screenshot = sc(region=(top_x, top_y, width, height))

        # convert screenshot to CV2 tpye
        screenshot = array(screenshot)
        screenshot = cvtColor(screenshot, COLOR_RGB2GRAY)

        # find tiles and click on it
        black_scan_click(screenshot, y_cord, time)
        # time = time + 1

        # show image for debpug
        cv2.imshow('capture', screenshot)
        if cv2.waitKey(1) == ord('q'):
            break

        if is_pressed('q'):
            break

    # end of the program
    # destroyAllWindows
    print("STOP!")
