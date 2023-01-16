if (process.env.NODE_ENV === "production") {
  module.exports = {
    CLIENT_ID: process.env.CLIENT_ID,
    MONGO_URL: process.env.MONGO_URL,
    ADMIN_KEY: process.env.ADMIN_KEY,
    PORT: process.env.PORT,
  };
} else {
  module.exports = require("./dev.config");
}
