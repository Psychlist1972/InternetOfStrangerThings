# My Internet of Stranger Things demo

This demo is something I built for a couple keynotes in Fall 2016, based upon the great Netflix series Stranger Things. It includes embedded technology, UWP speech recognition, some basic electronics, natural language understanding via LUIS, and a bot via the Microsoft Bot Framework. To round it all out, we even have a little bit of carpentry and wallpaper hanging. :)

![](https://c2.staticflickr.com/6/5604/30383497196_407b6e2f09_o.gif)

![](https://c1.staticflickr.com/9/8556/30302533082_fa330c3f92_o.gif)

The intent here is for this to be something you can learn from when building an intelligence-connected IoT device or Windows app.

This is not a ready-to-run project. Instead, it requires a number of steps before you can run it. In the solution, simply search for TODO in all files. There are several keys and IDs you'll need to add based on your LUIS and Bot Framework settings. Some are in code, and some are in the web.config. 

You will need:

- An Azure or other service to host the bot
- A Bot Framework account to register the bot
- A LUIS.ai account for natural language processing
- A bunch of LED Christmas lights (see blog series)
- Skills in working with wood and electronics
- An Arduino, a bunch of wire, and the LED controllers detailed in the blog series.
- A PC (or Windows Phone, although I haven't tested with that) to run the UWP app.

The bot itself will also need to be hosted in an internet-accessible location such as Microsoft Azure, and then registered on http://dev.botframework.com and then enabled on the Direct Line channel.

# Channel 9 Video

This video shows the wall in action and gives you a high-level overview of the steps required.

- https://channel9.msdn.com/Shows/Internet-of-Things-Show/IoTShowStrangerThings

# Blog Series

The blog series may be found here:

- Part 1: https://blogs.windows.com/buildingapps/2016/10/31/the-internet-of-stranger-things-wall-part-1-introduction-and-remote-wiring/
- Part 2: https://blogs.windows.com/buildingapps/2016/11/03/the-internet-of-stranger-things-wall-part-2-wall-construction-and-music/
- Part 3: https://blogs.windows.com/buildingapps/2016/11/04/the-internet-of-stranger-things-wall-part-3-voice-recognition-and-intelligence/

# Questions / comments?

Post here in discussion / issues, or tweet me @Pete_Brown http://twitter.com/Pete_Brown


