import os
import subprocess
from subprocess import call

for dirname, dirnames, filenames in os.walk('Assets\Blends'):
	# print path to all filenames.
	for filename in filenames:
		print(os.path.join(dirname, filename))
		if filename.endswith('.blend'):
			call(['blender', os.path.join(dirname, filename), '--background', '--python', 'aninames.py'])

print('Running Unity')
call([
		'C:\Program Files\Unity\Editor\Unity.exe',
		'-quit',
		'-batchmode',
		'-logFile',
		'C:\Workspace\\babc\log.txt',
		'-projectPath',
		'C:\Workspace\\babc',
		'-executeMethod',
		'BlendToBundlesConverter.convert'
	])

filepath = 'C:\Workspace\\babc\Assets\Bundles\\'
subprocess.Popen('explorer /select,"' + filepath + '"')