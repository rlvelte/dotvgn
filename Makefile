# In
SOLUTION := ./DotVgn.slnx
SAMPLES_DIR := ./samples
CONFIGURATION ?= Release
NUGET_PACKAGES_DIR ?=

# Out
RESULTS_DIR := ./build/tests
NUPKG_DIR := ./build/nupkgs
PUBLISH_DIR := ./build/publish
RPM_DIR := ./build/rpm
SBOM_DIR := ./build/sbom

# OS
ifeq ($(OS),Windows_NT)
  RUN := pwsh -NoProfile -File
  EXT := .ps1
else
  RUN := bash
  EXT := .sh
endif

# Export
export CONFIGURATION
export SOLUTION
export SAMPLES_DIR
export PUBLISH_DIR
export RPM_DIR

# Main
.DEFAULT_GOAL := build
.PHONY: format restore sbom build test coverage pack publish rpm mutate

format:
	dotnet format whitespace $(SOLUTION) \
		--verbosity diagnostic
	dotnet format $(SOLUTION) \
		--verbosity diagnostic

restore:
	dotnet restore $(SOLUTION) \
		$(if $(NUGET_PACKAGES_DIR),--packages $(NUGET_PACKAGES_DIR),) \
		--verbosity minimal

sbom: restore
	dotnet cyclonedx $(SOLUTION) \
		-o $(SBOM_DIR) \
		-F Json

build: restore
	dotnet build $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--verbosity minimal

test: build
	dotnet test --solution $(SOLUTION) \
		--no-build \
		--configuration $(CONFIGURATION) \
		--results-directory $(RESULTS_DIR) \
		-- --coverage --coverage-output-format cobertura --ignore-exit-code 8

coverage: test
	dotnet reportgenerator \
		-reports:$(RESULTS_DIR)/**/*.cobertura.xml \
		-targetdir:$(RESULTS_DIR)/coverage \
		-reporttypes:Html \
		-verbosity:Warning \
		-filefilters:-**/*.g.cs
	@echo "file://$(CURDIR)/$(RESULTS_DIR)/coverage/index.html"

pack:
	dotnet pack $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--output $(NUPKG_DIR) \
		--verbosity minimal

publish:
	$(RUN) scripts/publish$(EXT)

rpm: publish
ifeq ($(OS),Windows_NT)
	@echo "RPM packaging requires 'rpmbuild'." >&2
else
	$(RUN) scripts/rpm$(EXT)
endif

# Documentation
docs-build: build
	dotnet docfx docs/docfx.json

docs-serve: build
	dotnet docfx docs/docfx.json --serve

# Utility
clean:
ifeq ($(OS),Windows_NT)
	pwsh -NoProfile -Command "Remove-Item -Recurse -Force -ErrorAction SilentlyContinue build, StrykerOutput, docs/_site, docs/api"
else
	rm -rf build StrykerOutput docs/_site docs/api
endif
