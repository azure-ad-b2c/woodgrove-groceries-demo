const https = require("https");
const request = require("request");

https.globalAgent.keepAlive = true;

function createBadRequestErrorResponse(errorCode, errorMessage) {
    return createErrorResponse(400, errorCode, errorMessage);
}

function createErrorResponse(statusCode, errorCode, errorMessage) {
    return {
        status: statusCode,
        body: {
            "version": "1.0.0",
            "status": statusCode,
            "userCode": errorCode,
            "userMessage": errorMessage
        }
    };
}

function createInternalServerErrorErrorResponse() {
    return createErrorResponse(500, "InternalServerError", "An unexpected error has occurred.");
}

function createOKResponse(organization, user) {
    const res = {
        organizationId: organization.id,
        organizationDisplayName: organization.displayName,
        userId: user.id,
        userDisplayName: user.displayName
    };

    if (user.jobTitle) {
        res.userJobTitle = user.jobTitle;
    }

    if (user.mail) {
        res.userMail = user.mail;
    }

    return res;
}

function createUnauthorizedErrorResponse(errorCode, errorMessage) {
    return createErrorResponse(401, errorCode, errorMessage);
}

function getOrganization(context, accessToken, callback) {
    context.log.info(`Getting organization...`);

    request.get({
        url: "https://graph.microsoft.com/v1.0/organization",
        auth: {
            bearer: accessToken
        }
    }, (err, response, responseBody) => {
        if (err) {
            context.log.error(`FAILED: Getting organization. Error: ${err}.`);

            callback(err);
            return;
        }

        if (!isSuccessStatusCode(response.statusCode)) {
            const errorResult = JSON.parse(responseBody);

            context.log.warn(`FAILED: Getting organization. Error code: ${errorResult["odata.error"].code}. Error description: ${errorResult["odata.error"].message.value}.`);

            callback(null, {
                code: errorResult["odata.error"].code,
                message: errorResult["odata.error"].message.value
            });

            return;
        }

        const getOrganizationResponse = JSON.parse(responseBody);
        const organization = getOrganizationResponse.value[0];

        context.log.info(`SUCCEEDED: Got organization '${organization.id}'.`);

        callback(null, null, organization);
    });
}

function getUser(context, accessToken, callback) {
    context.log.info(`Getting user...`);

    request.get({
        url: "https://graph.microsoft.com/v1.0/me",
        auth: {
            bearer: accessToken
        }
    }, (err, response, responseBody) => {
        if (err) {
            context.log.error(`FAILED: Getting user. Error: ${err}.`);

            callback(err);
            return;
        }

        if (!isSuccessStatusCode(response.statusCode)) {
            const errorResult = JSON.parse(responseBody);

            context.log.warn(`FAILED: Getting user. Error code: ${errorResult["odata.error"].code}. Error description: ${errorResult["odata.error"].message.value}.`);

            callback(null, {
                code: errorResult["odata.error"].code,
                message: errorResult["odata.error"].message.value
            });

            return;
        }

        const user = JSON.parse(responseBody);

        context.log.info(`SUCCEEDED: Got user '${user.id}'.`);

        callback(null, null, user);
    });
}

function isSuccessStatusCode(statusCode) {
    return statusCode >= 200 && statusCode < 300;
}

module.exports = function (context, req) {
    var accessToken = null;
    var accessTokenError = false;

    if (req.query && req.query["access_token"]) {
        accessToken = req.query["access_token"];
    }

    if (req.body && req.body["access_token"]) {
        if (accessToken) {
            accessTokenError = true;
        }

        accessToken = req.body["access_token"];
    }

    if (req.headers && req.headers.authorization) {
        const authorizationParts = req.headers.authorization.split(' ');

        if (authorizationParts.length === 2 && authorizationParts[0] === "Bearer") {
            if (accessToken) {
                accessTokenError = true;
            }

            accessToken = authorizationParts[1];
        }
    }

    if (accessTokenError) {
        context.res = createBadRequestErrorResponse("invalid_request");
        context.done();
        return;
    }

    if (!accessToken) {
        context.res = createUnauthorizedErrorResponse();
        context.done();
        return;
    }

    getOrganization(context, accessToken, (err, error, organization) => {
        if (err || error) {
            context.res = createInternalServerErrorErrorResponse();
            context.done();
            return;
        }

        getUser(context, accessToken, (err, error, user) => {
            if (err || error) {
                context.res = createInternalServerErrorErrorResponse();
                context.done();
                return;
            }

            context.res = createOKResponse(organization, user);
            context.done();
        });
    });
};
