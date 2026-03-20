/**
 * Connect Client - shared utilities for all Jira Connect iframe pages.
 * Wraps the Atlassian Connect JS API (AP) and provides helpers for
 * calling our REST API from within the Jira iframe context.
 */
const TCM = (() => {
	'use strict';

	let _baseApiUrl = '';

	/**
	 * Initialize the Connect client. Call this from each page's DOMContentLoaded handler.
	 * @param {Function} [onReady] - Callback when AP is ready and context is loaded.
	 */
	function init(onReady) {
		// AP (Atlassian Plugin) is injected by all.js
		if (typeof AP === 'undefined') {
			console.error('Atlassian Connect JS (AP) not found. Are you running outside Jira?');
			return;
		}

		// Resize iframe to fit content
		AP.resize('100%', '100%');

		// Detect API base URL from the page origin
		_baseApiUrl = window.location.origin;

		if (onReady) {
			AP.context.getContext(function (context) {
				onReady(context);
			});
		}
	}

	/**
	 * Make an authenticated API call to our backend.
	 * Uses AP.request to ensure the JWT token is included.
	 * @param {string} path - API path (e.g., '/api/cycles?projectId=123')
	 * @param {Object} [options] - Fetch options (method, body, headers)
	 * @returns {Promise<Object>} Parsed JSON response
	 */
	async function api(path, options = {}) {
		const url = _baseApiUrl + path;
		const method = options.method || 'GET';
		const headers = { 'Content-Type': 'application/json', ...options.headers };

		return new Promise((resolve, reject) => {
			AP.request({
				url: url,
				type: method,
				contentType: 'application/json',
				data: options.body ? JSON.stringify(options.body) : undefined,
				success: function (response) {
					try {
						resolve(typeof response === 'string' ? JSON.parse(response) : response);
					} catch {
						resolve(response);
					}
				},
				error: function (xhr) {
					reject(new Error(`API ${method} ${path} failed: ${xhr.status} ${xhr.statusText}`));
				}
			});
		});
	}

	/**
	 * Get a query parameter from the current URL.
	 */
	function getQueryParam(name) {
		const params = new URLSearchParams(window.location.search);
		return params.get(name);
	}

	/**
	 * Render a status badge with appropriate color.
	 */
	function statusBadge(statusId, statusName) {
		const colors = {
			'-1': '#999',    // UnExecuted
			'1': '#36B37E',  // Pass
			'2': '#FF5630',  // Fail
			'3': '#FFAB00',  // WIP
			'4': '#6554C0',  // Blocked
			'5': '#00B8D9',  // PartiallyPassed
			'6': '#BF2600'   // FailedWithIssue
		};
		const color = colors[String(statusId)] || '#999';
		return `<span class="tcm-badge" style="background:${color}">${statusName || statusId}</span>`;
	}

	/**
	 * Show a Jira flag (toast notification).
	 */
	function showFlag(title, body, type) {
		AP.flag.create({
			title: title,
			body: body,
			type: type || 'info',
			close: 'auto'
		});
	}

	/**
	 * Show a loading spinner in the given container.
	 */
	function showLoading(container) {
		container.innerHTML = '<div class="tcm-loading"><div class="tcm-spinner"></div><p>Loading...</p></div>';
	}

	return { init, api, getQueryParam, statusBadge, showFlag, showLoading };
})();
