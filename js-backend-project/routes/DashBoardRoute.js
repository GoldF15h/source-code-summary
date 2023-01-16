const express = require("express");
const mongoose = require("mongoose");
const app = express();
const auth = require("../middleware/Authentication");
const superAuth = require("../middleware/SuperuserAuthentication");
const User = mongoose.model("user");

app.get("/dashboard", auth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "success";
  let message = null;

  try {
    const users = await User.find(
      {},
      {
        _id: 1,
        name: 1,
        score: 1,
      }
    );

    message = {
      users,
    };
  } catch (err) {
    statusCode = 400;
    statusMessage = "error";
    message = err.message;
  }

  res.status(statusCode).send({
    status: statusMessage,
    message,
  });
});

module.exports = app;
