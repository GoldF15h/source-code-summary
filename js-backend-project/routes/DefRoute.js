const express = require("express");
const app = express();
const auth = require("../middleware/Authentication");
const superAuth = require("../middleware/SuperuserAuthentication");
const cookieParser = require("cookie-parser");

const { OAuth2Client } = require("google-auth-library");
const CLIENT_ID = process.env.CLIENT_ID;
const client = new OAuth2Client(CLIENT_ID);

function tokenCheck(req, res, next) {
  // console.log("\n\n in tokencheck ");
  // console.log(req.cookies["session-token"]);
  // console.log("\n\n");

  let user = {};
  async function verify() {
    // console.log(req.cookies);
    token = req.cookies["session-token"];
    const ticket = await client.verifyIdToken({
      idToken: token,
      audience: CLIENT_ID,
    });
    const payload = ticket.getPayload();
    user.name = payload.name;
    user.email = payload.email;
    user.image = payload.picture;
  }
  verify()
    .then(() => {
      req.user = user;
      next();
    })
    .catch((err) => {
      // console.log("redirecting......");
      res.redirect("/api/login");
    });
}

app.set("view engine", "ejs");
// app.use(cookieParser());

app.get("/login", (req, res) => {
  res.render("login");
});

app.post("/login", (req, res) => {
  token = req.body.id_token;
  async function verify() {
    const ticket = await client.verifyIdToken({
      idToken: token,
      audience: CLIENT_ID,
    });
    const payload = ticket.getPayload();
    const userid = payload["sub"];
    // console.log(payload);
    // console.log(token);
  }
  verify()
    .then(() => {
      res.cookie("session-token", token);
      res.send("success");
    })
    .catch(console.error);

  // console.log("LOGIN TOKEN");
  // console.log(req.body.id_token);
  // res.send("hello");
});

app.get("/profile", tokenCheck, (req, res) => {
  const user = req.user;
  console.log(user);
  res.render("profile");
});

module.exports = app;
