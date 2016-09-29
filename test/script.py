# -*- coding: utf-8 -*-

import os
from sets import Set 

bundlesNames = []

for dirname, dirnames, filenames in os.walk('listas'):
	for filename in filenames:
		print 'Reading: ' + filename

		file = open('listas/' + filename)
		lines = file.readlines()

		for i in xrange(2, len(lines)):
			if lines[i] == '\n':
				break
			else:
				bundlesNames.append(lines[i][0:-1])

print bundlesNames

bundles = Set(bundlesNames)
print len(bundles)

for dirname, dirnames, filenames in os.walk('sinais'):
	for filename in filenames:
		if filename in bundles:
			os.rename('sinais/' + filename, 'checados/' + filename)
			bundles.remove(filename)

print '\nNao encontrados:'
print bundles