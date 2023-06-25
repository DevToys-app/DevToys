/// <binding BeforeBuild='devtoysJavaScript, devtoysSass' ProjectOpened='watchDevtoysSass, watchDevtoysJavaScript' />
/* eslint-disable @typescript-eslint/no-var-requires */
/* eslint-disable no-undef */
const gulp = require("gulp"); // or import * as gulp = require('gulp'
const cleanCSS = require("gulp-clean-css");
const rename = require("gulp-rename");
const dartSass = require("sass");
const gulpSass = require("gulp-sass");
const sass = gulpSass(dartSass);
const del = require("del");

var concat = require("gulp-concat");
var sourcemaps = require("gulp-sourcemaps");
var source = require("vinyl-source-stream");
var buffer = require("vinyl-buffer");
var rollup = require("@rollup/stream");

// Add support for require() syntax
var commonjs = require("@rollup/plugin-commonjs");

// Add support for importing = require(node_modules folder like import x = require('module-name'
var nodeResolve = require("@rollup/plugin-node-resolve");
// Cache needs to be initialized outside of the Gulp task 
var devtoysCache;
const paths = {
    devtoysScss: ["./Assets/sass/**/*.scss", "./Components/**/*.scss", "./Pages/**/*.scss"],
    devtoysScssOut: "./wwwroot/css",
    devtoysJavascript: "./Assets/javascript",
    devtoysJavascriptOut: "./wwwroot/js"
};

function devtoysSass() {
    return gulp.src(paths.devtoysScss)
        .pipe(sass())
        .pipe(concat("devtoys.g.css"))
        .pipe(gulp.dest(paths.devtoysScssOut))
        .pipe(cleanCSS({ compatibility: "*", inline: ["all"], level: 2 }))
        .pipe(gulp.dest(paths.devtoysScssOut));
}

function watchDevtoysSass() {
    gulp.watch(paths.devtoysScss, devtoysSass);
}

function devtoysJavascript(cb) {
    return rollup({
        input: `${paths.devtoysJavascript}/index.esm.js`,
        cache: devtoysCache,
        plugins: [
            commonjs,
            nodeResolve
        ],
        output: {
            format: "iife",
            sourcemap: true,
            name: "devtoys"
        }
    }).on("bundle", function (bundle) {
        devtoysCache = bundle;
    }).pipe(source(`${paths.devtoysJavascript}/index.esm.js`))
        .pipe(buffer())
        .pipe(sourcemaps.init({ loadMaps: true }))
        .pipe(sourcemaps.write("."))
        .pipe(gulp.dest(paths.devtoysJavascriptOut))
        .pipe(rename("devtoys.g.js", { extname: ".js" }))
        .pipe(gulp.dest(paths.devtoysJavascriptOut))
        .on("end", function () {
            console.log("end");
            del([`${paths.devtoysJavascriptOut}/Assets/**`]);
        });
}

function watchDevtoysJavaScript() {
    gulp.watch(`${paths.devtoysJavascript}/**/*.js`, devtoysJavascript);
}

function devtoysCleanup(cb) {
    return del(
        [
            "./Assets/sass/**/*.css",
            "./Components/**/*.css",
            "./Pages/**/*.css",
            "./Assets/javascript/**/*.js",
            "./Components/**/*.js",
            "./Pages/**/*.js",
            "./wwwroot/**/*.g.js",
            "./wwwroot/**/*.g.css"
        ]);
}

exports.devtoysSass = devtoysSass;
exports.watchDevtoysSass = watchDevtoysSass;
exports.devtoysJavaScript = devtoysJavascript;
exports.watchDevtoysJavaScript = watchDevtoysJavaScript;
exports.devtoysCleanup = devtoysCleanup;
