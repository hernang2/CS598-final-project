import os
import pandas as pd
from flair.data import Corpus, Dictionary
from flair.datasets import ColumnCorpus
from pathlib import Path

from flair.models import LanguageModel
from flair.trainers.language_model_trainer import LanguageModelTrainer, TextCorpus

ROOT_DIR = os.path.realpath(os.path.join(os.path.dirname(__file__), '..'))
DATA_DIR = os.path.realpath(os.path.join(os.path.dirname(__file__), '..\\..\\pharmaconer'))

if __name__ == '__main__':
    corpus_type = 'train'
    relative_path = 'bio\\text-corpus'
    data_folder = os.path.join(DATA_DIR, relative_path)

    is_forward_lm = True
    dictionary: Dictionary = Dictionary.load('chars')

    language_model = LanguageModel(dictionary,
                                   is_forward_lm,
                                   hidden_size=256,
                                   nlayers=3)
    print(data_folder)
    corpus = TextCorpus(data_folder,
                        dictionary=dictionary,
                        forward=True,
                        character_level=True)

    trainer = LanguageModelTrainer(language_model, corpus)

    trainer.train('resources/taggers/language_model',
                  sequence_length=160,
                  learning_rate=0.1,
                  mini_batch_size=8,
                  max_epochs=150)
