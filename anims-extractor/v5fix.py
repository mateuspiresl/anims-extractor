import os

def refactor_bundles(path):
	_refactor(False, path)
	print('v5fix Done!')

def _refactor(parent_name, path):
	for parent, dirnames, filenames in os.walk(path):
		# print '\tAt ' + parent

		for filename in filenames:
			if filename == parent_name or filename.endswith('.manifest'):
				# print 'Removing ' + os.path.join(path, filename)
				os.remove(os.path.join(path, filename))
			elif parent_name != 'Verificar':
				# print 'Renaming ' + os.path.join(path, filename) + ' to ' + os.path.join(path, filename.upper())

				try:
					os.rename(os.path.join(path, filename), os.path.join(path, filename.upper()))
				except:
					# print 'File not found'
					pass
		
		for dirname in dirnames:
			dirpath = os.path.join(path, dirname)
			_refactor(dirname, dirpath)

			try:
				if os.listdir(dirpath) == []:
					# print 'Removing empty dir ' + dirpath
					os.remove(dirpath)
			except:
				pass