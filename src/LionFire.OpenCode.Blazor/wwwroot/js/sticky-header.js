// Sticky Header JavaScript Module
// Uses Intersection Observer for efficient sticky detection

const observers = new Map();

/**
 * Initialize sticky header behavior using Intersection Observer
 * @param {HTMLElement} sentinel - The sentinel element to observe
 * @param {object} dotNetRef - DotNet reference for callbacks
 * @param {number} topOffset - Top offset in pixels
 */
export function initStickyHeader(sentinel, dotNetRef, topOffset = 0) {
    if (!sentinel) return;

    // Remove existing observer if present
    destroyStickyHeader(sentinel);

    const options = {
        root: null, // viewport
        rootMargin: `-${topOffset + 1}px 0px 0px 0px`,
        threshold: [0, 1]
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            // When sentinel is not intersecting, the header is stuck
            const isStuck = !entry.isIntersecting;
            dotNetRef.invokeMethodAsync('OnStickyStateChanged', isStuck);
        });
    }, options);

    observer.observe(sentinel);
    observers.set(sentinel, { observer, dotNetRef });
}

/**
 * Clean up the sticky header observer
 * @param {HTMLElement} sentinel - The sentinel element
 */
export function destroyStickyHeader(sentinel) {
    if (!sentinel) return;

    const data = observers.get(sentinel);
    if (data) {
        data.observer.disconnect();
        observers.delete(sentinel);
    }
}

/**
 * Scroll to a specific message element
 * @param {string} elementId - The ID of the element to scroll to
 * @param {string} behavior - Scroll behavior ('smooth' or 'instant')
 */
export function scrollToMessage(elementId, behavior = 'smooth') {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior, block: 'start' });
    }
}

/**
 * Get the current scroll position
 * @param {HTMLElement} container - The scroll container (or null for window)
 * @returns {object} - { scrollTop, scrollHeight, clientHeight }
 */
export function getScrollPosition(container) {
    if (container) {
        return {
            scrollTop: container.scrollTop,
            scrollHeight: container.scrollHeight,
            clientHeight: container.clientHeight
        };
    }
    return {
        scrollTop: window.scrollY || document.documentElement.scrollTop,
        scrollHeight: document.documentElement.scrollHeight,
        clientHeight: window.innerHeight
    };
}

/**
 * Scroll to top of container
 * @param {HTMLElement} container - The scroll container (or null for window)
 * @param {string} behavior - Scroll behavior
 */
export function scrollToTop(container, behavior = 'smooth') {
    if (container) {
        container.scrollTo({ top: 0, behavior });
    } else {
        window.scrollTo({ top: 0, behavior });
    }
}

/**
 * Scroll to bottom of container
 * @param {HTMLElement} container - The scroll container (or null for window)
 * @param {string} behavior - Scroll behavior
 */
export function scrollToBottom(container, behavior = 'smooth') {
    if (container) {
        container.scrollTo({ top: container.scrollHeight, behavior });
    } else {
        window.scrollTo({ top: document.documentElement.scrollHeight, behavior });
    }
}
