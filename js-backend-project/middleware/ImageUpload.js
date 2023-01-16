const util = require("util");
const multer = require("multer");
const fs = require("fs");
const maxSize = 2 * 1024 * 1024;
const IMAGE_DIR = "/image";
const TASK_DIR = "/task";

const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    cb(null, process.cwd() + IMAGE_DIR);
  },
  filename: (req, file, cb) => {
    // console.log(req.params);
    const newName = "user_image_" + req.params.id + "." + "png";
    req.body.url_image = newName;
    cb(null, newName);
  },
});

const uploadFile = multer({
  storage: storage,
  limits: { fileSize: maxSize },
  fileFilter: (req, file, cb) => {
    if (
      file.mimetype == "image/png" ||
      file.mimetype == "image/jpg" ||
      file.mimetype == "image/jpeg"
    ) {
      cb(null, true);
    } else {
      cb(null, false);
      return cb(new Error("Only .png, .jpg and .jpeg format allowed!"));
    }
  },
}).single("image");

const unlinkAsync = util.promisify(fs.unlink);

let ImageUpload = util.promisify(uploadFile);

module.exports = { ImageUpload, unlinkAsync };
