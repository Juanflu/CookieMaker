var express = require('express');
var router = express.Router();
var http = require('http');

/* GET home page. */
var UIApiHostname = process.env.UIApiHostname;
if(UIApiHostname == undefined)
{
    UIApiHostname = '127.0.0.1';
}

router.get('/', function(req, res, next) 
{
    var options = 
    {
        hostname: UIApiHostname,
        port: 8000,
        path: '/api/cookie',
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    };
      
    var reqQuery = http.request(options, function(resQuery) 
    {
        resQuery.setEncoding('utf8');
        resQuery.on('data', function (data) 
        {
            console.log(data);
            var jsonObject = JSON.parse(data);
            res.render('index', { cookies: jsonObject });
        });
    });

    reqQuery.on('error', function(e) 
    {
        console.log('problem with request: ' + e.message);
    });

    reqQuery.end();
});

/* POST to Add User Entry */
router.post("/addCookies", function(req, res) 
{
    // Get our form values. These rely on the "name" attributes
    var numberOfCookies = req.body.numberOfCookies;
    console.log("post received: %s", numberOfCookies);

    var options = 
    {
        hostname: UIApiHostname,
        port: 8000,
        path: '/api/cookie',
        method: 'POST',
        headers: 
        {
            'Content-Type': 'application/json'
        },
    };
      
    const reqPost = http.request(options, (res) => 
    {
        console.log('statusCode: ' + res.statusCode)
      
        res.on('data', (d) => 
        {
            console.log(String.fromCharCode.apply(null, d));
        })
    });
      
    reqPost.on('error', (error) => 
    {
        console.error(error);
    })

    reqPost.write(numberOfCookies);
    reqPost.end();

    res.redirect('./');
});

module.exports = router;
