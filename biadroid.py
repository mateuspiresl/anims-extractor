import os
import subprocess
from subprocess import call
import sys
import re
import shutil

def clearBlends():
	print('Cleaning Assets/Blends...')
	for dirname, dirnames, filenames in os.walk('Assets/Blends'):
		for filename in filenames:
			os.remove(os.path.join(dirname, filename))
		for dirn in dirnames:
			shutil.rmtree(os.path.join(dirname, dirn))
	print('Assets/Blends is empty.')


root = os.path.dirname(os.path.realpath(__file__))


blendsDir = raw_input('Folder: ')

yesPattern = re.compile('[yY]')
toContinue = ''

if os.path.isdir('Assets/Blends'):
	numFiles = len([name for name in os.listdir('Assets/Blends')])

	if numFiles > 0:
		answer = raw_input('Assets/Blends (alguns files/dirs) will be cleared, prompt y/Y to continue or anything to quit: ')
		if (answer != 'y' and answer != 'Y'):
			sys.exit()
		else:
			clearBlends()
else:
	os.mkdir('Assets/Blends')
	print('Assets/Blends created!')



if not os.path.isdir('LOGS'):
	os.mkdir('LOGS')
	print('LOGS created!')

print('Checking ' + blendsDir + '...')

for dirname, dirnames, filenames in os.walk(blendsDir):
	for filename in filenames:
		if filename.endswith('.blend'):
			print('')
			print('Found: ' + filename)
			path = os.path.join(dirname, filename)

			blendpath = root + '/Assets/Blends/' + filename
			print('Moving ' + path + ' to ' + blendpath + '...')
			os.rename(path, blendpath)
			
			print('Getting animation names...')
			call(['blender', blendpath, '--background', '--python', 'aninames.py'])
			
			print('Running converter...')
			call([
					'C:\Program Files\Unity\Editor\Unity.exe',
					'-quit',
					'-batchmode',
					'-logFile',
					root + '\LOGS\LOG__' + filename[:-6] + '.txt',
					'-projectPath',
					root,
					'-executeMethod',
					'BlendToBundlesConverter.convert'
				])

			print('Success!')
			clearBlends()

filepath = 'C:\Workspace\\babc\Assets\Bundles\\'
subprocess.Popen('explorer /select,"' + filepath + '"')