import os

import flair
from typing import List

from flair.data import Dictionary
from flair.datasets import ColumnCorpus
from flair.local_embeddings import TransformerWordEmbeddings
from flair.trainers import ModelTrainer, TextCorpus
from flair.models import SequenceTagger
from flair.embeddings import TokenEmbeddings, WordEmbeddings, StackedEmbeddings, FlairEmbeddings, BytePairEmbeddings

ROOT_DIR = os.path.realpath(os.path.join(os.path.dirname(__file__), '..'))
DATA_DIR = os.path.realpath(os.path.join(os.path.dirname(__file__), '..\\..\\pharmaconer'))

corpus_type = 'train'
relative_path = 'bio\\text-corpus'
data_folder = os.path.join(DATA_DIR, relative_path)

tag_type = 'ner'

dictionary: Dictionary = Dictionary.load('chars')

corpus = ColumnCorpus(data_folder,
                    dictionary=dictionary,
                    forward=True,
                    character_level=True)

embedding_types: List[TokenEmbeddings] = [
    # WordEmbeddings('glove'),
    #TransformerWordEmbeddings('bert-base-uncased'),
    BytePairEmbeddings(language="es"),
    FlairEmbeddings('resources/taggers/language_model_quick/best-lm.pt'),
]

embeddings: StackedEmbeddings = StackedEmbeddings(embeddings=embedding_types)

tagger: SequenceTagger = SequenceTagger(hidden_size=256,
                                        embeddings=embeddings,
                                        tag_dictionary=dictionary,
                                        tag_type=tag_type,
                                        use_crf=True)

trainer: ModelTrainer = ModelTrainer(tagger, corpus)

trainer.train('/content/model/flair',
              learning_rate=0.00005,
              mini_batch_size=32,
              max_epochs=30,
              embeddings_storage_mode='cpu')
