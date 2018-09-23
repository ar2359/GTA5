import argparse
from cv2 import cvtColor
from cv2 import COLOR_BGR2RGB
from cv2 import imwrite
from cv2 import resize
import numpy as np
import csv
#import win32con
import win32gui
from PIL import ImageGrab
from time import sleep

def main(filename):
	
	wind=win32gui.FindWindow(None,"Grand Theft Auto V")
	(x,y,h,w)=win32gui.GetWindowRect(wind)
		
	screen=cvtColor(np.array(ImageGrab.grab(bbox=(x,y,h,w))),COLOR_BGR2RGB)
	
	s=[x.strip() for x in filename.split(',')]
		
	imwrite(s[0], screen)
		
	imwriter = csv.writer(open('D:\\GTATestingSet\\000\\000.csv', 'a')) #replace this
	imwriter.writerow([s[1],s[2],s[3],s[4],s[5],s[6]])
	
	
if __name__=='__main__':
	parser=argparse.ArgumentParser(description='Say Hello')
	parser.add_argument('name',help='enter the filename',type=str)
	args=parser.parse_args()
	main(args.name)