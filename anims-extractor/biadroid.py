import os
import subprocess
from subprocess import call
import sys
import re
import shutil
import v5fix

def clearBlends():
	print('Cleaning Assets/Blends...')
	for dirname, dirnames, filenames in os.walk('Assets/Blends'):
		for filename in filenames:
			os.remove(os.path.join(dirname, filename))
		for dirn in dirnames:
			shutil.rmtree(os.path.join(dirname, dirn))
	print('Assets/Blends is empty.')


root = os.path.dirname(os.path.realpath(__file__))


def gen(blends, prompt):
	if os.path.isdir('Assets/Blends'):
		numFiles = len([name for name in os.listdir('Assets/Blends')])

		if numFiles > 0:
			if prompt:
				answer = input('Assets/Blends (alguns files/dirs) will be cleared, prompt y/Y to continue or anything to quit: ')
				if (answer != 'y' and answer != 'Y'):
					sys.exit()
				else:
					clearBlends()
			else:
				clearBlends()
	else:
		os.mkdir('Assets/Blends')
		print('Assets/Blends created!')



	if not os.path.isdir('LOGS'):
		os.mkdir('LOGS')
		print('LOGS created!')

	print('Checking ' + blends + '...')

	for dirname, dirnames, filenames in os.walk(blends):
		for filename in filenames:
			if filename.endswith('.blend'):
				print('')
				print('Found: ' + filename)
				path = os.path.join(dirname, filename)

				blendpath = root + '/Assets/Blends/' + filename
				print('Moving ' + path + ' to ' + blendpath + '...')
				os.rename(path, blendpath)

				animspath = root + '/Assets/Anims'
				if os.path.isdir(animspath):
					print ('Removing ' + animspath) 
					shutil.rmtree(animspath)
				
				print('Getting animation names...')
				call(['blender', blendpath, '--background', '--python', 'aninames.py'])
				
				print('Running converter...')
				call([
						r'C:\Program Files\Unity\Editor\Unity.exe',
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


if __name__ == '__main__':
	blendsDir = input('Folder: ')

	yesPattern = re.compile('[yY]')
	toContinue = ''

	gen(blendsDir, True)
	v5fix.refactor_bundles('./Bundles')