import os
import bpy
import codecs
from sets import Set

# write names into a file next to the blend
file = codecs.open('_aninames.txt', 'w', 'utf-8')

ignore = Set(["_configMaoDir", "_configMaoEsq", "_orientacaoDir", "_orientacaoEsq", "_pontoArticula_Dir", "_pontoArticula_Esq", "CDA:ObIpo.001", "DOWN_", "Facial", "PosePadraoNova","_2vl_CONTATO","POSE-NEUTRA"])

for obj in bpy.data.actions:
	if obj.name not in ignore:
	    file.write("%s\n" % (obj.name))

file.close()