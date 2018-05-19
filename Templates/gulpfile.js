var gulp = require('gulp');
var concat = require('gulp-concat');
var cssmin = require('gulp-cssmin');
var inject = require('gulp-inject');
var removeCode = require('gulp-remove-code');

gulp.task('default', function () {
    // Concatenate and minify the style files.
    var cssStream = gulp.src(['./src/css/bootstrap-reboot.min.css', './src/css/style.css'])
        .pipe(concat('all.css'))
        .pipe(cssmin());

    // Copy the font files.
    gulp.src(['./src/font/roboto/*.*'])
        .pipe(gulp.dest('./dist/font/roboto'));

    // Copy the image files.
    gulp.src(['./src/images/*.*'])
        .pipe(gulp.dest('./dist/images'));

    // Inject the concatenated, minified, style files into each of the HTML files and remove any external files.
    return gulp.src(['./src/*.html'])
        .pipe(inject(cssStream, {
            transform: function (filePath, file) {
                return '<style>' + file.contents.toString('utf8') + '</style>';
            }
        }))
        .pipe(removeCode({
            production: true
        }))
        .pipe(gulp.dest('./dist'));
});
