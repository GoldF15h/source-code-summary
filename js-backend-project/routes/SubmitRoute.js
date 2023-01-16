const express = require("express");
const mongoose = require("mongoose");
const app = express();
const auth = require("../middleware/Authentication");
const superAuth = require("../middleware/SuperuserAuthentication");
const Flag = mongoose.model("flag");
const User = mongoose.model("user");
const FinishedTask = mongoose.model("finishedTask");

app.post("/submit/:id", auth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "submit success";
  let message = null;

  try {
    const flag = await Flag.find(
      {
        id: req.params.id,
        flag: req.body.flag,
      },
      {
        _id: 1,
        id: 1,
        flag: 1,
      }
    );
    // console.log(flag.length);
    if (flag.length != 0) {
      const _finshedTask = await FinishedTask.find(
        {
          taskId: req.params.id,
          userId: req.body.id,
        },
        {}
      );
      // console.log(_finshedTask);
      if (_finshedTask.length == 0) {
        // console.log("not Found");
        // console.log(req.body.id);
        try {
          const curUser = await User.find(
            {
              _id: req.body.id,
            },
            {
              score: 1,
            }
          );
          // console.log(curUser);
          // console.log(curUser[0].score);
          const newScore = curUser[0].score + 1 || 0;
          await User.findOneAndUpdate(
            {
              _id: req.body.id,
            },
            {
              score: newScore,
            },
            {
              new: true,
              projection: {
                score: 1,
              },
            }
          );
          const newFinishedTask = new FinishedTask({
            taskId: req.params.id,
            userId: req.body.id,
          });
          await newFinishedTask.save();
          message = true;
        } catch (err) {
          statusCode = 404;
          statusMessage = "error";
          message = "NOT FOUND";
        }
      } else {
        message = "TASK FINISHED";
      }
    } else {
      message = false;
    }
  } catch (err) {
    statusCode = 404;
    statusMessage = "error";
    message = "NOT FOUND";
  }

  res.status(statusCode).send({
    status: statusMessage,
    message,
  });
});

app.post("/createflag", superAuth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "flag create success";
  let message = null;

  try {
    // console.log("req body", req.body, "end");
    const newFlag = new Flag({
      id: req.body.id,
      flag: req.body.flag,
    });
    // console.log(newUser);
    await newFlag.save();

    statusCode = 200;
    statusMessage = "success";
    message = "flag created success";
  } catch (error) {
    statusCode = 400;
    statusMessage = "error";
    message = error.message;
  }

  res.status(statusCode).send({
    status: statusMessage,
    message,
  });
});

module.exports = app;
