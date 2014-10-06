phosphorus-five
===============

phosphorus five is a web application framework for mono and asp.net.  phosphorus 
adds the fun back into creating web apps, and allows you to build software that 
outlasts the pyramids.  phosphorus is;

* secure
* lightweight
* beautiful
* flexible
* intuitive

#### secure

the web, and especially javascript, is insecure by design.  phosphorus fixes this, 
by making sure all your business logic stay on your server.  phosphorus also ties 
everything down by default, and makes your systems safe against intrusions

*"with phosphorus five, you sleep at night"*

#### lightweight

phosphorus is lightweight in all regards.  the javascript sent to the client is 
tiny, there is no unnecessary html rendered, the http traffic is tiny, and the 
server is not clogged with expensive functionality.  phosphorus solves the problems 
that needs to be solved and nothing more

*"with phosphorus five, your creations can grow into heaven"*

#### beautiful

phosphorus features beautiful code, and allows you to create beautiful code yourself.  
the class hierarchy is easy to understand.  the javascript and html rendered is easy 
to read, and conforms to all known standards.  phosphorus facilitates for you 
creating beautiful end results

*"with phosphorus five, your creations can be beautiful"*

#### flexible

phosphorus is highly flexible.  it allows you to easily create your own logic, 
overriding what you need to override, and not worry about the rest.  with phosphorus, 
you decide what html and javascript is being rendered back to the client and how 
your class hierarchy should be designed

*"with phosphorus five, you are the boss"*

#### intuitive

phosphorus is easy to understand, and contains few things you do not already know how 
to use.  it builds on top of asp.net, c#, html, javascript and json.  if you know c#, 
asp.net, and have used libraries such as jQuery or prototype.js before, then you 
don't need to learn anything new to get started

*"with phosphorus five, your first hunch is probably right"*

## getting started with phosphorus.ajax

create a reference to *"lib/phosphorus.ajax.dll"* in your asp.net web application

then modify your web.config, and make sure it has something like this inside its 
*"system.web"* section

```xml
<system.web>
  <pages>
    <controls>
      <add 
        assembly="phosphorus.ajax" 
        namespace="phosphorus.ajax.widgets" 
        tagPrefix="pf" />
    </controls>
  </pages>
</system.web>
```

then either inherit your page from AjaxPage, or implement the IAjaxPage interface, 
before you create a literal widget, by adding the code below in your .aspx markup

```xml
<pf:Literal
    runat="server"
    id="hello"
    Tag="strong"
    onclick="hello_onclick">
    click me
</pf:Literal>
```

then add the following code in your codebehind

```csharp
protected void hello_onclick (pf.Literal sender, EventArgs e)
{
    sender.innerHTML = "hello world";
}
```

if you wish to have more samples for how to use phosphorus.ajax, you can check out the 
*"phosphorus.ajax.samples"* project by opening up the *"phosphorus.sln"* file

## the literal and container widgets

in phosphorus.ajax there is only two types of web controls.  there is the *"Literal"* 
class, and the *"Container"* class.  by combining these two classes, you can create 
html markup you wish

the **literal** widget is for controls that contains text or html, and allows you to 
change its content through the *"innerHTML"* property.  notice that the literal widget 
can have html elements inside of it, but these will be treated as client side html, 
and not possible to change on the server side, except by modifying the parent literal 
control.  everything inside of the beginning and the end of your literal widget in 
your .aspx markup will be treated as pure html, and not parsed as controls in any ways

the **container** widget can have child controls, which will be parsed in the .aspx 
markup as controls, and possible to reference on the server side, and modify 
in your server side code through its *"Controls"* collection.  everything inside of 
the beginning and the end of your container widget in your .aspx markup, will be 
treated as a web control

altough the comparison does not do justify the features of the phosphorus widgets, 
you can think of the literal widget as the *"Label"* equivalent, and the container 
widget as the *"Panel"* equivalent

#### modifying your widgets

the first thing you have to decide when creating a widget, is what html tag you wish 
to render it with.  this is set through the *"Tag"* property of your widget.  you can 
render any widget with any html tag you wish, but remember, that you have to make sure 
what you're rendering is html compliant.  phosphorus.ajax supports the html5 standard 
100%, but it also supports the html500 standard, even though nobody knows how that 
looks like today, and it is probably wise to stick to the html5 standard for now

then you can start adding attributes to your widget.  this is done by simply adding 
any attribute you wish, either directly in the markup of your .aspx page, or by using 
the index operator overload in c#.  the framework will automatically take care of 
serializing your attributes correctly back to the client

below is an example of how to create a video html5 element using a literal widget

```xml
<pf:Literal
    runat="server"
    id="video"
    Tag="video"
    width="640"
    onclick="video_click"
    controls>
    <source 
        src="http://download.blender.org/peach/trailer/trailer_1080p.ogg" 
        type="video/ogg" />
    your browser blows!
</pf:Literal>
```

you can also modify or add any attribute you wish in the codebehind by using something 
like this, and the engine will automatically keep track of which items are dirty and 
needs to be sent back to the client

```csharp
protected void video_click (Literal literal, EventArgs e)
{
    // notice how you save a cast operation here ...
    literal ["width"] = "1024";
}
```

you can modify any attribute you wish on your widgets, by using the index operator.  
phosphorus.ajax will automatically keep track of what needs to be sent from the 
server to the client





