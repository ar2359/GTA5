import csv
s=input("Enter the number of csv files to be converted\n(Enter 0 for only 000.csv , 1 for 000.csv and 001.csv and so on:")

#ensure first line of each csv file is ,,,,


for j in range(0,int(s)+1):


	fil='00' + str(j)

	f=open(fil+'.csv',newline='')

	csv_reader=csv.reader(f)



	text_file = open(fil+'.txt', "a")

	for i, row in enumerate(csv_reader):
		if i %2 !=0 and row[0]!='0':
			print("Writing Line")
			text_file.write(row[1]+".png "+row[2]+"\n")
			#print(row)
			
			
	print("\nWriting done for "+str(j))		
	text_file.close()
	f.close()
