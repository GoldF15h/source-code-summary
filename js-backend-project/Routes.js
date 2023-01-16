const express = require('express');
const app = express();

// load model
const userModel = require('./model/User');
const flagModel = require('./model/Flag');
const taskModel = require('./model/Task');
const FinishedTask = require('./model/FinishedTask');

// use model
// app.use(xxx);

// load route
const DefRoute = require('./routes/DefRoute');
const UserRoute = require('./routes/UserRoute');
const DashBoardRoute = require('./routes/DashBoardRoute');
const SubmitRoute = require('./routes/SubmitRoute');
const TaskRoute = require('./routes/TaskRoute');

// use route
app.use(DefRoute);
app.use(UserRoute);
app.use(DashBoardRoute);
app.use(SubmitRoute);
app.use(TaskRoute);

module.exports = app;
