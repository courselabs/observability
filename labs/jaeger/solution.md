
web -> authz

http.route = check/{userId}/{documentAction}
http.url = http://fulfilment-authz/check/0421/Submit

authz -> blog.sixeyed.com

web -> api

http.url	
http://fulfilment-api/document


api - logs

handler.class_simple_name = DocumentsController	
handler.method_name	= submitDocument