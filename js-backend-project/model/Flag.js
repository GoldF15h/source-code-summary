const mongoose = require("mongoose");
const { Schema } = mongoose;

const flagSchema = new Schema({
  id: {
    type: Number,
  },
  flag: {
    type: String,
  },
});

mongoose.model("flag", flagSchema);
