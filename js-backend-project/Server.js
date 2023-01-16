const express = require("express");
const path = require("path");
require("dotenv").config({ path: path.resolve(__dirname, ".env") });
const PORT = process.env.PORT || 5000;
const app = express();
const mongoose = require("mongoose");
const bodyParser = require("body-parser");
const IMAGE_DIR = "./image";
const TASK_DIR = "./task";
const fs = require("fs");
const config = require("./config");
const cookieParser = require("cookie-parser");
const cors = require("cors");

app.use(
  cors({
    origin: ["http://localhost:3000", "http://localhost:3001"],
  })
);

// console.log(process.env);
connectToDB().catch((err) => console.log(err));
async function connectToDB() {
  await mongoose.connect(config.MONGO_URL);
}

// app.use(bodyParser());
app.use(cookieParser());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use("/api", require("./Routes"));

app.listen(config.PORT, () => {
  folderManager();
  console.log("app listening at PORT " + PORT);
});

const folderManager = () => {
  if (!fs.existsSync(IMAGE_DIR)) {
    try {
      fs.mkdir(IMAGE_DIR, { recursive: true }, (err) => {
        console.log("Photo directory is created.");
      });
    } catch (err) {
      console.log("fail to create floder \n" + err.message);
    }
  }
  if (!fs.existsSync(TASK_DIR)) {
    try {
      fs.mkdir(TASK_DIR, { recursive: true }, (err) => {
        console.log("Task directory is created.");
      });
    } catch (err) {
      console.log("fail to create floder \n" + err.message);
    }
  }
};
