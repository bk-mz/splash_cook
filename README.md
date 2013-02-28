splash_cook
===========

Keywords: Splash Screen, PNG, BMP, Parametrize, Assembly Version, Custom Text

Based on http://kentb.blogspot.ru/2012/12/wpf-splash-screens.html.

It's a console utility for Splash Screen bitmap generation with custom provided text. It's the utility that should be used in your CI pre-build step.

You do something like before build:

`splash_cook Resources/splash_template.png Resources/Splash.png --show-instantly=no 304,64,Century_Gothic,30,Bold,#1a4780,MY_TEXT1 100,500,Verdana,10,Normal,#00000,MY_TEXT2`

Result you get is Splash.png with text printed on top of template image. Just make sure that your client-program depends on cooked splash.
