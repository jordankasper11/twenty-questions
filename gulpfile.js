var gulp = require('gulp');
var sass = require('gulp-sass');

gulp.task('sass', function (cb) {
    gulp
        .src('./scss/*.scss')
        .pipe(sass())
        .pipe(gulp.dest('./css/'));
    cb();
});

gulp.task('font-awesome', function () {
    return gulp
        .src('node_modules/font-awesome/fonts/*')
        .pipe(gulp.dest('fonts'))
})

gulp.task('default',
    gulp.series('sass', 'font-awesome', function (cb) {
        gulp.watch('./scss/**/*.scss', gulp.series('sass'));
        cb();
    })
);