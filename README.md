# Netflix Next With Sitecore Cortex - Session Demo Code 
### Pre-Requisites 
1. Ensure you have a fresh new instance of Sitecore installed of version 9.1.1 or up 
2. Ensure all your services and sites are working.  It is very important that XConnect Url is up and running on your environment 
3. Subscription for UNogs is needed, Free subscription is an option, you can see more details here [Pricing Options for Unogs](https://rapidapi.com/unogs/api/unogs/pricing)
4. Front end plugin is needed for the MVC views to render properly, you can purchase a license at [Theme Forest] (https://themeforest.net/item/flixgo-online-movies-tv-shows-cinema-html-template/22538349)


## Steps to get the Demo working 

1. Install the package located under (location goes here after check in).  This contains some Marketing Items which are the basis for some of Cortex functions.  This would also contain all Templates, Views, etc., needed by the Demo application 
2. Publish the site. 
3. Pass your UNogs API key in GetDataFromUnogs method on line 74 in MovieRecommendationsProvider class  httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "your UNOGS Key");
4. Ensure to swap the Connection Strings on rebusSettings.xml with your environment specific Messaging Catalog Connection String. 
5. Run Netflixnext.Client console application and select '1' to generate the model JSON
6. Model is generated in the bin\debug folder with name NetflixNextMovie, 1.0.  Copy this file over to the below places 
   - \xconnectroot\App_Data\Models
   - \IndexWorkerRoot\App_data\Models
   - \ProcessingEngineRoot\App_data\Models
7. Ensure the following dll's and corresponding pdb from solution         ---
    - NetflixSitecore.dll
    - NetflixNext.ProcessingEngine.Extensions.dll
    - NetflixNext.xConnect.Extensions
    - NetflixNext.Common
    
    are copied to all the below locations 

   - WebsiteRoot\bin
   - XConnectRoot\bin
   - IndexWorkerRoot
   - ProcessingEngine Root
   
8. Restart all services and sites 

## See Recommendations in Action 

1. Go to front end facing website via url: websitedomain/login and provide your sitecore credentials to login 
2. You should be redirected to Listing Page which would showcase different type of Listings of Movies. 
3. Click around your favorite movies from the listings of different types 
4. Run NetflixNext.Client and select option #2 - This should run Sitecore Cortex Engine to fetch recommendations based on the activity of the user 
5. Once the application processing is complete.  Close the Client.
6. Reload the listing page - you should now see recommendations based on user activity


