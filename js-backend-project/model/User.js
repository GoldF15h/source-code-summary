const mongoose = require("mongoose");
const { Schema } = mongoose;

const userSchema = new Schema({
  name: {
    type: String,
  },
  score: {
    type: Number,
  },
});

mongoose.model("user", userSchema);
