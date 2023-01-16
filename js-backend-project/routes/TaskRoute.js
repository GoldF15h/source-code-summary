const express = require("express");
const mongoose = require("mongoose");
const app = express();
const auth = require("../middleware/Authentication");
const superAuth = require("../middleware/SuperuserAuthentication");
const Task = mongoose.model("task");
var path = require("path");
const TASK_DIR = "./task";

app.get("/task/:id", auth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "success";
  let message = null;

  try {
    const task = await Task.find(
      {
        id: req.params.id,
      },
      {
        _id: 1,
        id: 1,
        title: 1,
      }
    );

    message = {
      task,
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

app.get("/task", auth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "success";
  let message = null;

  try {
    const task = await Task.find(
      {},
      {
        _id: 1,
        id: 1,
        title: 1,
      }
    );

    message = {
      task,
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

app.put("/task", superAuth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "put success";
  let message = null;
  try {
    const _title = req.body.title || "empty";
    const _file = req.body.file || "empty";

    if (req.body.id) {
      const upadateTask = await Task.findOneAndUpdate(
        {
          id: req.body.id,
        },
        {
          title: _title,
        },
        {
          new: true,
          projection: {
            title: 1,
          },
        }
      );

      if (upadateTask) {
        message = {
          upadateTask,
        };
      } else {
        statusCode = 404;
        statusMessage = "error";
        message = "user not fond";
      }
    }
  } catch (err) {
    // console.log(err.message);
    statusCode = 400;
    statusMessage = "error";
    message = "bad req";
  }
  res.status(statusCode).send({
    status: statusMessage,
    message,
  });
});

app.post("/task", superAuth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "task create success";
  let message = null;

  try {
    // console.log("req body", req.body, "end");
    const newTask = new Task({
      id: req.body.id,
      title: req.body.title,
    });
    // console.log(newUser);
    await newTask.save();

    statusCode = 200;
    statusMessage = "success";
    message = "task create success";
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

app.get("/task/file/:id", auth, function (req, res) {
  let statusCode = 200;
  let statusMessage = "upload success";
  let message = null;
  // console.log(__dirname);
  var options = {
    root: path.join(TASK_DIR),
  };

  // console.log(req.params.id);
  var fileName = req.params.id + ".txt";
  res.sendFile(fileName, options, function (err) {
    if (err) {
      statusMessage = "File Not Foud";
      statusCode = 404;
      res.status(statusCode).send({
        status: statusMessage,
        message,
      });
    } else {
      console.log("Sent:", fileName);
    }
  });
});

module.exports = app;
