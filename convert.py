import os
from subprocess import call

#call(['ls', '-l'])

for dirname, dirnames, filenames in os.walk('Assets\Blends'):
	# print path to all filenames.
	for filename in filenames:
		print(os.path.join(dirname, filename))
		call(['blender', os.path.join(dirname, filename), '--background', '--python', 'aninames.py'])

print('Running Unity')
call([
		'C:\Program Files\Unity\Editor\Unity.exe',
		'-quit',
		'-batchmode',
		'-logFile',
		'C:\Workspace\wikilibras-player\log.txt',
		'-projectPath',
		'C:\Workspace\wikilibras-player\playercore_blend',
		'-executeMethod',
		'BlendToBundlesConverter.convert'
	])