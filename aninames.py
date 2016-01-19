import os
import bpy
import codecs

# write names into a file next to the blend
file = codecs.open(os.path.splitext(bpy.data.filepath)[0] + '_aninames.txt', 'w', 'utf-8')

for obj in bpy.data.actions:
    file.write("%s\n" % (obj.name))

file.close()