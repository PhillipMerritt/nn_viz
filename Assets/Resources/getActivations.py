import keract
import imageio
from keras.models import load_model
import sys

model_path, input_path = sys.argv[1], sys.argv[2]

model = load_model(model_path)

img = imageio.imread(input_path)

activations = keract.get_activations(model, img)

keract.display_activations(activations, cmap=None, save=True, directory='./activations/', data_format='channels_last', reshape_1d_layers=True)