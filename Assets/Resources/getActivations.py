import keract
import imageio
from keras.models import load_model
import sys
import numpy as np
import os

""" from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.side_channel import (
    SideChannel,
    IncomingMessage,
    OutgoingMessage,
)
import numpy as np
import uuid """

def start(model_path, input_path):
    model = load_model(model_path)

    filenames = os.listdir("Assets/Resources/activations/")
    for fn in filenames:
        os.remove("Assets/Resources/activations/{}".format(fn))

    filenames = os.listdir(input_path)

    filenames = sorted(filenames)

    for i, fn in enumerate(filenames):
        img = np.array([imageio.imread("{}/{}".format(input_path, fn))])

        max_val = np.max(img)
        if max_val > 1:
            img = img / 255

        activations = keract.get_activations(model, img)

        keract.persist_to_json_file(activations, 'Assets/Resources/activations/activations{}.json'.format(i))

""" class CustomSideChannel(SideChannel):

    def __init__(self) -> None:
        super().__init__(uuid.UUID("621f0a70-4f87-11ea-a6bf-784f4387d1f7"))

    def on_message_received(self, msg: IncomingMessage) -> None:
        print(msg)

        modelPath, inputPath = msg.split('\n')
        start(modelPath, inputPath)
        self.send_string("json created")


    def send_string(self, data: str) -> None:
        # Add the string to an OutgoingMessage
        msg = OutgoingMessage()
        msg.write_string(data)
        # We call this method to queue the data we want to send
        super().queue_message_to_send(msg)

channel = CustomSideChannel()

env = UnityEnvironment(side_channels=[channel])
env.reset() """


try:
    print("getting paths")
    modelPath, inputPath = sys.argv[1], sys.argv[2]
    start(modelPath, inputPath)
    print("activation json saved")
except Exception as e:
    print(str(e))