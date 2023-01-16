const mongoose = require("mongoose");
const { Schema } = mongoose;

const finishedTaskSchema = new Schema({
  taskId: {
    type: String,
  },
  userId: {
    type: String,
  },
});

mongoose.model("finishedTask", finishedTaskSchema);
