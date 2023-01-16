const config = require("../config");

const superUserAuthentication = (req, res, next) => {
  if (req.body.WnsMJVXNKD == config.ADMIN_KEY) {
    next();
  } else {
    res.status(400).send({
      status: "BAD REQ",
      message: "Authentication Failed",
    });
  }
};

module.exports = superUserAuthentication;
