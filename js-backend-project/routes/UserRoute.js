const express = require("express");
const mongoose = require("mongoose");
const app = express();
const auth = require("../middleware/Authentication");
const superAuth = require("../middleware/SuperuserAuthentication");
const User = mongoose.model("user");
const fileManage = require("../middleware/ImageUpload");
var path = require("path");
const IMAGE_DIR = "./image";

app.get("/user/:id", auth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "success";
  let message = null;

  try {
    const users = await User.find(
      {
        _id: req.params.id,
      },
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

app.get("/user", auth, async (req, res) => {
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

app.put("/user/:id", superAuth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "put success";
  let message = null;
  try {
    const _name = req.body.name || "empty";
    const upadateUser = await User.findOneAndUpdate(
      {
        _id: req.params.id,
      },
      {
        name: _name,
      },
      {
        new: true,
        projection: {
          name: 1,
        },
      }
    );

    if (upadateUser) {
      message = {
        upadateUser,
      };
    } else {
      statusCode = 404;
      statusMessage = "error";
      message = "user not fond";
    }
  } catch (err) {
    console.log(err.message);
    statusCode = 400;
    statusMessage = "error";
    message = "bad req";
  }
  res.status(statusCode).send({
    status: statusMessage,
    message,
  });
});

app.post("/user", superAuth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "usercreate success";
  let message = null;

  // console.log(req.file);

  try {
    newName = req.body.name || "";
    const findUsers = await User.find(
      {
        name: newName,
      },
      {
        _id: 1,
        name: 1,
        score: 1,
      }
    );
    // console.log(findUsers.length);
    if (findUsers.length == 0) {
      try {
        // console.log("req body", req.body, "end");
        const newUser = new User({
          name: newName,
          score: 0,
        });
        // console.log(newUser);
        await newUser.save();

        statusCode = 200;
        statusMessage = "success";
        message = newUser.id;
      } catch (error) {
        statusCode = 400;
        statusMessage = "BAD REQ";
        message = "";
      }
    } else {
      statusCode = 200;
      statusMessage = "success";
      message = findUsers[0].id;
    }
  } catch (error) {
    statusCode = 400;
    statusMessage = "BAD REQ";
    message = "";
  }

  res.status(statusCode).send({
    status: statusMessage,
    message,
  });
});

app.post("/user/uploadImage/:id", auth, async (req, res) => {
  let statusCode = 200;
  let statusMessage = "upload success";
  let message = null;

  try {
    // console.log("req body", req.body, "end");
    // console.log(req.file);
    await fileManage.ImageUpload(req, res);

    statusCode = 200;
    statusMessage = "success";
    message = "image upload success";
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

app.get("/user/image/:id", auth, function (req, res) {
  let statusCode = 200;
  let statusMessage = "upload success";
  let message = null;
  // console.log(__dirname);
  var options = {
    root: path.join(IMAGE_DIR),
  };

  var fileName = "user_image_" + req.params.id + ".png";
  res.sendFile(fileName, options, function (err) {
    if (err) {
      fileName = "DefImage.png";
      res.sendFile(fileName, options, function (err) {
        if (err) {
          statusCode = 400;
          statusMessage = "Image Not Found";
          res.status(statusCode).send({
            status: statusMessage,
            message,
          });
        }
      });
    }
  });
});

module.exports = app;
