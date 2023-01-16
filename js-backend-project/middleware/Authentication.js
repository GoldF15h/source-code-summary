const { OAuth2Client } = require("google-auth-library");
const config = require("../config");
const CLIENT_ID = config.CLIENT_ID;
const client = new OAuth2Client(CLIENT_ID);

const authentication = (req, res, next) => {
  // let user = {};
  async function verify() {
    // console.log("test token");
    // console.log(req.cookies);
    token = req.cookies["session-token"];
    // console.log("test token");
    const ticket = await client.verifyIdToken({
      idToken: token,
      audience: CLIENT_ID,
    });
    const payload = ticket.getPayload();
    // user.name = payload.name;
    // user.email = payload.email;
    // user.image = payload.picture;
  }

  verify()
    .then(() => {
      console.log("AUTH PASS");
      // console.log(user);
      // req.user = user;
      next();
    })
    .catch((err) => {
      res.status(400).send({
        status: "BAD REQ",
        message: "Authentication Failed",
      });
    });
};

module.exports = authentication;
