const mongoose = require("mongoose");
const { Schema } = mongoose;

const taskSchema = new Schema({
  id: {
    type: Number,
  },
  title: {
    type: String,
  },
});

mongoose.model("task", taskSchema);
