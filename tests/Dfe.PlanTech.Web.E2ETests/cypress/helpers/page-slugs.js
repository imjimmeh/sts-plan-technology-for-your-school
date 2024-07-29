export const selfAssessmentSlug = "/self-assessment";

export const FindPageForSlug = ({ slug, dataMapper }) => {
    for (const [id, page] of dataMapper.pages.entries()) {
        if (page.fields.slug == slug) {
            return page;
        }
    }
    return null;
}