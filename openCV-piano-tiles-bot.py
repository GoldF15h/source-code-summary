import pytesseract
import os
import cv2
import pyautogui
import numpy
import PIL
import time
import keyboard

if __name__ == "__main__" :
    #path to tesseract.exe
    pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
    counter = 0
    while True:

        # take Screenshot
        screenshot = pyautogui.screenshot(region=(0,260, 800, 70))
        
        # convert screenshot to CV2 tpye
        cv_img = numpy.array(screenshot)
        cv_img = cv2.cvtColor(cv_img,cv2.COLOR_RGB2GRAY)
        cv2.imshow('capture',cv_img) 
        if cv2.waitKey(1) == ord('q'):
            break
            
        result = pytesseract.image_to_string(screenshot,lang='eng')
        result = result + ' '
        counter = counter + len(result.split())
        pyautogui.click(100, 450)
        pyautogui.write(result)

        # if counter > 350 :
        #     break

        if keyboard.is_pressed('esc') :
            break
        
        