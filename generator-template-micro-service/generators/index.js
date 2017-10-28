'use strict';
var yeoman = require('yeoman-generator');
var chalk = require('chalk');
var yosay = require('yosay');

module.exports = yeoman.generators.Base.extend({
  //Configurations will be loaded here.
  //Ask for user input
  prompting: function () {	 	
    var done = this.async();
    this.prompt([
	{
      type    : 'input',
      name    : 'name',
      message: 'Enter project name (ie fusionsvc-template):'
	  //default : 'fusionsvc-template'
    }, {
      type    : 'input',
      name    : 'svcDesc',
      message : 'provide a short description of service:'
	  //default : 'You did not enter a description'
    }, {
      type    : 'input',
      name    : 'unitTestName',
      message : 'Test project name (ie TemplateSvcTest):'
	  //default : 'TemplateSvcTest'
    }, {
      type    : 'input',
      name    : 'dispatcherName',
      message : 'Dispatch project name (ie TemplateServiceDispatcher):'
	  //default : 'TemplateServiceDispatcher'
    }, {
      type    : 'input',
      name    : 'projectNameSpace',
      message : 'Sub namespace (ie Template to make it Sovos.Template.Model):'
	  //default : 'Template'
    }], function(answers) {
      this.props = answers;
      this.log(answers.name);
      this.log(answers.svcDesc);
      this.log(answers.unitTestName);
      this.log(answers.dispatcherName);
	  this.log(answers.projectNameSpace);
      done();
    }.bind(this));
  },
  //Writing Logic here
  writing: {
    //Copy the configuration files
    config: function() { 	  
      this.fs.copyTpl(
        this.templatePath('_README.md'),
        this.destinationPath('README.md'), {
          name: this.props.name,
		  svcDesc: this.props.svcDesc
        }
      );
      this.fs.copyTpl(
        this.templatePath('_template-service.sln'),
        this.destinationPath(this.props.name + '.sln'), {
          test: this.props.unitTestName,
		  dispatch: this.props.dispatcherName
		}
      );
      this.fs.copy(
        this.templatePath('add_submodules.sh'),
        this.destinationPath('add_submodules.sh')
      );
      this.fs.copy(
        this.templatePath('AssemblyVersionInfo.cs'),
        this.destinationPath('AssemblyVersionInfo.cs')
      );
      this.fs.copy(
        this.templatePath('gitignore'),
        this.destinationPath('.gitignore')
      );
      this.fs.copy(
        this.templatePath('gitmodules'),
        this.destinationPath('.gitmodules')
      );
      this.fs.copy(
        this.templatePath('gitignore'),
        this.destinationPath('.gitignore')
      );
	  
	  //copy build scripts
      this.fs.copy(
        this.templatePath('_build/_build.cmd'),
        this.destinationPath('build/build.cmd')
      );
      this.fs.copyTpl(
        this.templatePath('_build/_build_internal.cmd'),
        this.destinationPath('build/build_internal.cmd'),
		{
			name: this.props.name
		}
      );
      this.fs.copy(
        this.templatePath('_build/_clean.cmd'),
        this.destinationPath('build/clean.cmd')
      );
      this.fs.copy(
        this.templatePath('_build/_jenkins.cmd'),
        this.destinationPath('build/jenkins.cmd')
      );
      this.fs.copyTpl(
        this.templatePath('_build/_publish.cmd'),
        this.destinationPath('build/publish.cmd'),
		{
			name: this.props.name,
			dispatch: this.props.dispatcherName
		}
      );
      this.fs.copy(
        this.templatePath('_build/_replace.ps1'),
        this.destinationPath('build/replace.ps1')
      );
      this.fs.copyTpl(
        this.templatePath('_build/_setup.cmd'),
        this.destinationPath('build/setup.cmd'),
		{
          test: this.props.unitTestName
		}
      );
      this.fs.copyTpl(
        this.templatePath('_build/_sign.cmd'),
        this.destinationPath('build/sign.cmd'),
		{
			namespace: this.props.projectNameSpace,
			dispatch: this.props.dispatcherName
		}
      );
      this.fs.copy(
        this.templatePath('_build/_teardown.cmd'),
        this.destinationPath('build/teardown.cmd')
      );
      this.fs.copyTpl(
        this.templatePath('_build/_TemplateServiceDispatcher.cjson.erb'),
        this.destinationPath('build/' + this.props.dispatcherName + '.cjson.erb'),
		{
			dispatch: this.props.dispatcherName
		}
      );
      this.fs.copy(
        this.templatePath('_build/_test.cmd'),
        this.destinationPath('build/test.cmd')
      );
      this.fs.copy(
        this.templatePath('_build/_version.cmd'),
        this.destinationPath('build/version.cmd')
      );
    },

    //Copy application files
    app: function() {
	  //----------------------------------------------------------------Model
	  //Model.Capability
      this.fs.copyTpl(
        this.templatePath('_Model/_Capability/_DIInputDto.cs'),
        this.destinationPath('Model/Capability/DIInputDto.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Model/_Capability/_TemplateMessage.cs'),
        this.destinationPath('Model/Capability/' + this.props.projectNameSpace + 'Message.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  //Model.Exceptions
      this.fs.copyTpl(
        this.templatePath('_Model/_Exceptions/_TemplateException.cs'),
        this.destinationPath('Model/Exceptions/' + this.props.projectNameSpace + 'Exception.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  
	  //Model.Repositories
      this.fs.copyTpl(
        this.templatePath('_Model/_Repositories/_IRepositoryFactory.cs'),
        this.destinationPath('Model/Repositories/IRepositoryFactory.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Model/_Repositories/_ISecurityAnswerRepository.cs'),
        this.destinationPath('Model/Repositories/ISecurityAnswerRepository.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  
	  //Model.Services
      this.fs.copyTpl(
        this.templatePath('_Model/_Services/_ITemplateService.cs'),
        this.destinationPath('Model/Services/I' + this.props.projectNameSpace + 'Service.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Model/_Services/_TemplateDispatchInterface.cs'),
        this.destinationPath('Model/Services/' + this.props.projectNameSpace + 'DispatchInterface.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Model/_Services/_TemplateService.cs'),
        this.destinationPath('Model/Services/' + this.props.projectNameSpace + 'Service.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  
	  //Model csproj
      this.fs.copyTpl(
        this.templatePath('_Model/_Model.csproj'),
        this.destinationPath('Model/Model.csproj'),
		{
			namespace: this.props.projectNameSpace,
			dispatch: this.props.dispatcherName
		}
      );
	  
	  
	  //----------------------------------------------------------------ModelUT
	  //ModelUT.Properties
      this.fs.copy(
        this.templatePath('_ModelUT/_Properties/_AssemblyInfo.cs'),
        this.destinationPath('ModelUT/Properties/AssemblyInfo.cs')
      );  
	  //ModelUT.Services
      this.fs.copyTpl(
        this.templatePath('_ModelUT/_Services/_Stubs/_RepositoryFactory.cs'),
        this.destinationPath('ModelUT/Services/Stubs/RepositoryFactory.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_ModelUT/_Services/_Stubs/_SecurityAnswerRepository.cs'),
        this.destinationPath('ModelUT/Services/Stubs/SecurityAnswerRepository.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_ModelUT/_Services/_TemplateServiceTest.cs'),
        this.destinationPath('ModelUT/Services/' + this.props.projectNameSpace + 'ServiceTest.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  //ModelUT csproj
      this.fs.copyTpl(
        this.templatePath('_ModelUT/_ModelUT.csproj'),
        this.destinationPath('ModelUT/ModelUT.csproj'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  
	  //----------------------------------------------------------------Persistence
      this.fs.copyTpl(
        this.templatePath('_Persistence/_BaseGateway.cs'),
        this.destinationPath('Persistence/BaseGateway.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Persistence/_Persistence.csproj'),
        this.destinationPath('Persistence/Persistence.csproj'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Persistence/_RepositoryFactory.cs'),
        this.destinationPath('Persistence/RepositoryFactory.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_Persistence/_SecurityAnswerRepository.cs'),
        this.destinationPath('Persistence/SecurityAnswerRepository.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );
	  
	  //----------------------------------------------------------------PersistenceUT
      this.fs.copyTpl(
        this.templatePath('_PersistenceUT/_config/_Alias.config'),
        this.destinationPath('PersistenceUT/config/Alias.config'),
		{
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_config/_properties.config'),
        this.destinationPath('PersistenceUT/config/properties.config')
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_config/_properties.tag.config'),
        this.destinationPath('PersistenceUT/config/properties.tag.config')
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_config/_providers.config'),
        this.destinationPath('PersistenceUT/config/providers.config')
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_config/_SecurityAnswer.config'),
        this.destinationPath('PersistenceUT/config/SecurityAnswer.config')
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_config/_sqlMap.config'),
        this.destinationPath('PersistenceUT/config/sqlMap.config')
      );
	  
      this.fs.copy(
        this.templatePath('_PersistenceUT/_Properties/_AssemblyInfo.cs'),
        this.destinationPath('PersistenceUT/Properties/AssemblyInfo.cs')
      );
	   
      this.fs.copy(
        this.templatePath('_PersistenceUT/_App.config'),
        this.destinationPath('PersistenceUT/App.config')
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_Asserter.cs'),
        this.destinationPath('PersistenceUT/Asserter.cs')
      );
	  this.fs.copy(
        this.templatePath('_PersistenceUT/_BaseTest.cs'),
        this.destinationPath('PersistenceUT/BaseTest.cs')
      );	  
      this.fs.copy(
        this.templatePath('_PersistenceUT/_Mapper.cs'),
        this.destinationPath('PersistenceUT/Mapper.cs')
      );
      this.fs.copy(
        this.templatePath('_PersistenceUT/_PersistenceUT.csproj'),
        this.destinationPath('PersistenceUT/PersistenceUT.csproj')
      );
      this.fs.copyTpl(
        this.templatePath('_PersistenceUT/_SecurityAnswerRepositoryTest.cs'),
        this.destinationPath('PersistenceUT/SecurityAnswerRepositoryTest.cs'),
		{
			namespace: this.props.projectNameSpace
		}
      );	  
	  //----------------------------------------------------------------TemplateServiceDispatcher
      this.fs.copyTpl(
        this.templatePath('_TemplateServiceDispatcher/_config/_Alias.config'),
        this.destinationPath(this.props.dispatcherName + '/config/Alias.config'),
		{
			dispatch: this.props.dispatcherName
		}
      ); 
	  
	  
      this.fs.copyTpl(
        this.templatePath('_TemplateServiceDispatcher/_Properties/_AssemblyInfo.cs'),
        this.destinationPath(this.props.dispatcherName + '/Properties/AssemblyInfo.cs'),
		{
			dispatch: this.props.dispatcherName.replace('Dispatcher', '')
		}
      );
	  
      this.fs.copyTpl(
        this.templatePath('_TemplateServiceDispatcher/_App.config'),
        this.destinationPath(this.props.dispatcherName + '/App.config'),
		{
			dispatch: this.props.dispatcherName,
			namespace: this.props.projectNameSpace.toLowerCase()
		}
      );
      this.fs.copyTpl(
        this.templatePath('_TemplateServiceDispatcher/_Program.cs'),
        this.destinationPath(this.props.dispatcherName + '/Program.cs'),
		{
			dispatch: this.props.dispatcherName,
			dispatchShort: this.props.dispatcherName.replace('Dispatcher', ''),
			namespace: this.props.projectNameSpace
		}
      );
      this.fs.copyTpl(
        this.templatePath('_TemplateServiceDispatcher/_TemplateServiceDispatcher.csproj'),
        this.destinationPath(this.props.dispatcherName + '/' + this.props.dispatcherName + '.csproj'),
		{
			dispatch: this.props.dispatcherName
		}
      );
      this.template(
        this.templatePath('_TemplateServiceDispatcher/_TemplateServiceDispatcher.exe.config.erb'),
        this.destinationPath(this.props.dispatcherName + '/' + this.props.dispatcherName + '.exe.config.erb'),
		{
			dispatch: this.props.dispatcherName
		}
      );
	  
	  //----------------------------------------------------------------TemplateSvcTest
      this.fs.copyTpl(
        this.templatePath('\_TemplateSvcTest/_Properties/_AssemblyInfo.cs'),
        this.destinationPath(this.props.unitTestName + '/Properties/AssemblyInfo.cs'),
		{
			dispatch: this.props.dispatcherName.replace('Dispatcher', '')
		}
      );
	  
      this.fs.copyTpl(
        this.templatePath('\_TemplateSvcTest/_App.config'),
        this.destinationPath(this.props.unitTestName + '/App.config'),
		{
			namespace: this.props.projectNameSpace.toLowerCase()
		}
      );
      this.fs.copyTpl(
        this.templatePath('_TemplateSvcTest/_Program.cs'),
        this.destinationPath(this.props.unitTestName + '/Program.cs'),
		{
			test: this.props.unitTestName
		}
      );
      this.fs.copyTpl(
        this.templatePath('_TemplateSvcTest/_TemplateSvcTest.csproj'),
        this.destinationPath(this.props.unitTestName + '/' + this.props.unitTestName + '.csproj'),
		{
			test: this.props.unitTestName
		}
      );
    }
  },

    end: function () {
        var that = this;
		var done = this.async();
		this.conflicter.force = true;
		
		this.spawnCommand('add_submodules.sh').on('close', function () {
			that.fs.delete('add_submodules.sh');
			process.chdir('build');
			that.spawnCommand('setup.cmd').on('close', function () {
				that.spawnCommand('build.cmd').on('close', function () {
					done();
				});
				done();
			});
			done();
		});
    }  
});
