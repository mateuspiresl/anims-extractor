import os
import biadroid


root = raw_input('Folder: ')

for dirname, dirnames, filenames in os.walk(root):
	for dirn in dirnames:
		blends = dirname + "\\" + dirn
		print "\n\nAUTOGEN " + blends + "\n\n"
		
		biadroid.gen(blends, False)
		os.rename("Bundles", "Bundles_" + dirn)

	break