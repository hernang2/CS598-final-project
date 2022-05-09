import glob, os, ntpath
from pathlib import Path
from spacy.lang.es import Spanish

nlp = Spanish()

labels_names = ['PROTEINAS', 'NORMALIZABLES', 'UNCLEAR', 'NO_NORMALIZABLES']

ROOT_DIR = os.path.realpath(os.path.join(os.path.dirname(__file__), '..'))
DATA_DIR = os.path.realpath(os.path.join(os.path.dirname(__file__), '..\\..\\pharmaconer'))

corpus_type = 'train'
relative_path = '{}-set_1.1\\{}\\subtrack1'.format(corpus_type, corpus_type)
data_folder = os.path.join(DATA_DIR, relative_path)

os.chdir(data_folder)
for file_path in glob.glob("*.txt"):
    file_name = ntpath.basename(file_path)
    text = Path(file_path).read_text(encoding="UTF-8")
    doc = nlp(text)
    tokens = [token.text for token in doc]
    with open(os.path.join(DATA_DIR, 'bio\\{}'.format(corpus_type), file_name), 'w', encoding="utf-8") as filehandle:
        for token in tokens:
            filehandle.write('%s\n' % token)
