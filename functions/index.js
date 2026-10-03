const { onDocumentCreated } = require("firebase-functions/v2/firestore");
const { defineString } = require("firebase-functions/params");
const { initializeApp } = require("firebase-admin/app");
const { getFirestore } = require("firebase-admin/firestore");

initializeApp();
const db = getFirestore();

// Both overridable via functions/.env without touching code - see that file.
const notifyEmail = defineString("NOTIFY_EMAIL", {
  default: "boltonwalkingandoutdoors@yahoo.com",
});
const adminCcEmail = defineString("ADMIN_CC_EMAIL", {
  default: "johncampbellhiggens@googlemail.com",
});

// Close to both the Firestore multi-region (Belgium/Netherlands) and the
// Trigger Email extension's London region, avoiding cross-region egress.
const REGION = "europe-west1";

/**
 * Writes a document to "mail" - the Trigger Email extension watches this
 * collection and sends the actual email via SendGrid. Admin SDK writes
 * bypass Firestore rules entirely, so this works regardless of the public
 * create-only rule on "mail" (that rule exists only for a possible future
 * direct client write, not needed by this function).
 *
 * Always cc's the admin address; additionally cc's the submitter themselves
 * if they gave an email, so they get a copy of what they sent.
 */
function queueEmail(subject, bodyLines, submitterEmail) {
  const cc = [adminCcEmail.value()];
  if (submitterEmail) cc.push(submitterEmail);

  return db.collection("mail").add({
    to: notifyEmail.value(),
    cc,
    message: {
      subject,
      text: bodyLines.filter((line) => line !== null).join("\n"),
    },
  });
}

exports.onRouteSubmissionCreated = onDocumentCreated(
  { document: "route-submissions/{id}", region: REGION },
  async (event) => {
    const data = event.data.data();

    await queueEmail(`New route submission: ${data.name || "Unnamed route"}`, [
      `Venue: ${data.venue || ""}`,
      `Difficulty: ${data.difficulty || ""}`,
      data.shortDescription ? `Distance/ascent/duration: ${data.shortDescription}` : null,
      data.terrainNotes ? `Notes: ${data.terrainNotes}` : null,
      data.gpxFileName ? `GPX file attached: ${data.gpxFileName}` : null,
      data.routeUrl ? `Route URL: ${data.routeUrl}` : null,
      Array.isArray(data.photoUrls) && data.photoUrls.length > 0
        ? `${data.photoUrls.length} photo(s) attached`
        : null,
      "",
      `Submitted by: ${data.submitterName || ""} (${data.submitterEmail || ""})`,
      "",
      "Review it in the admin page's Submissions tab.",
    ], data.submitterEmail);
  }
);

exports.onFeedbackCreated = onDocumentCreated(
  { document: "feedback/{id}", region: REGION },
  async (event) => {
    const data = event.data.data();

    await queueEmail(`New ${data.category || ""} feedback from ${data.submitterName || ""}`, [
      `Category: ${data.category || ""}`,
      data.rating ? `Rating: ${data.rating}/5` : null,
      data.appFeedbackType ? `Type: ${data.appFeedbackType}` : null,
      data.eventLabel ? `Walk: ${data.eventLabel}` : null,
      data.walkQuality ? `How did you find this walk?: ${data.walkQuality}` : null,
      data.walkOrganisation ? `How did you find the organisation?: ${data.walkOrganisation}` : null,
      data.bookingExperience ? `How did you find booking?: ${data.bookingExperience}` : null,
      data.comments ? `Comments: ${data.comments}` : null,
      "",
      `Submitted by: ${data.submitterName || ""} (${data.submitterEmail || ""})`,
      `Device: ${data.devicePlatform || ""} ${data.deviceModel || ""}`,
      "",
      "Review it in the admin page's Feedback tab.",
    ], data.submitterEmail);
  }
);
