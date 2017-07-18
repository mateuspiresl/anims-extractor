import sys
import os
import shutil
from sets import Set

ignoredFiles = Set(['Default Take', '_configMaoDir', '_configMaoEsq', '_orientacaoDir', '_orientacaoEsq', '_pontoArticula_Dir', '_pontoArticula_Esq', 'CDA:ObIpo.001', 'DOWN_', 'Facial', 'PosePadraoNova','_2vl_CONTATO','POSE-NEUTRA'])
platformFolders = Set(['ANDROID', 'IOS', 'STANDALONE', 'WEBGL'])

def moveFiles(destiny, path):
	for maindir, subdirs, files in os.walk(path):
		print 'Checking ' + maindir + ' files'
		for file in files:
			filePath = path + '/' + file
			fileDestiny = destiny + '/' + file

			if file in ignoredFiles:
				os.remove(filePath)
				'Deleting ' + filePath

			elif not os.path.isfile(fileDestiny):
				print '\t' + filePath
				os.rename(filePath, destiny + '/' + file)
			

		print 'Checking ' + maindir + ' folders'
		for dir in subdirs:
			if dir in platformFolders:
				moveFiles(destiny + '/' + dir, maindir + '/' + dir)
			else:
				moveFiles(destiny, maindir + '/' + dir)

			if deleteIfEmpty(maindir + '/' + dir):
				print 'Deleting ' + maindir + '/' + dir

		break

def deleteIfEmpty(path):
	for maindir, subdirs, files in os.walk(path):
		if len(subdirs) == 0 and len(files) == 0:
			shutil.rmtree(path)
			return True

		return False

if __name__ == '__main__':
	folder = sys.argv[1]
	destiny = 'merge_' + folder

	if not os.path.isdir(destiny):
		os.mkdir(destiny)
		os.mkdir(destiny + '/ANDROID')
		os.mkdir(destiny + '/IOS')
		os.mkdir(destiny + '/STANDALONE')
		os.mkdir(destiny + '/WEBGL')

	moveFiles(destiny, folder)