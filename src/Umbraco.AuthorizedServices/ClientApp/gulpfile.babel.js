import gulp from 'gulp';

import { paths } from './gulp/config';
import { js } from './gulp/js';

const copy = () => gulp.src(paths.src).pipe(gulp.dest(paths.dest));

gulp.task('copy', copy);

gulp.task('watch', done => {
  console.log('Watching for changes... Press Ctrl+C to exit.');
  gulp.watch(paths.src, gulp.parallel(copy));
  gulp.watch(paths.js, gulp.parallel(js));
  done();
});

export const build = gulp.task('build', gulp.parallel(copy, js));
export const dev = gulp.task('dev', gulp.series('build', 'watch'));
