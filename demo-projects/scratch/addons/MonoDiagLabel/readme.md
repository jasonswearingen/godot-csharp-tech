# MonoDiagLabel

I will eventually get around to posting this in the asset library, but if you are writing a C# game this label may be useful.

it's a port of the FPS Label to C# and adds more details, such as GC collection info.. 

Addionally, I hope this is a good reference for anyone who wants to create their own C# addon.


add this to **the root node** of your scene to get advanced performance information, including GC details.

## Quantiles (how to read label)
Framerates are displayed as Quantiles (0th to 4th).  

For example:  ```insideTreeMs Quantiles: 0.1 / 2.4 / 2.6 / 5.8 / 15.2 (803 samples)```
- ```insideTreeMs``` shows how much time is taken inside the scene tree, in milliseconds
- ```0.1```:  the 0th Quartile, this is the time taken by the fastest frame during the sample interval
- ```2.4```: the 1st Quartile, aprox 25% frames took less than 2.4ms, while 75% took more than 2.4ms.
- ```2.6```: the 2nd Quartile, IE the Median.  (50% frames took less than 2.6ms)
- ```5.8```: the 3rd Quartile, aprox 75% frames took less than 5.8ms.  25% more.
- ```15.2```: the 4th Quartile, the time that the slowest frame took
- ```(803 samples)``` how many frames were measured


For more information on Quartiles, see: https://en.wikipedia.org/wiki/Quantile#Examples


# update frequency 

Samples are taken every frame (call to ```._Process()```).

You can customize the label update via the Node inspector (click on the node after you add it to your scene)

# Performance note

- Best to have label update every 0.5 seconds or slower (default is 2 seconds).  This is to reduce pressure on the GC  (displaying text allocates junk string objects).